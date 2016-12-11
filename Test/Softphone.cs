using System;
using System.Collections.Generic;
using System.Threading;
using System.Collections;
using Ozeki.VoIP;
using Ozeki.Media;

namespace DialerNS
{

    // class SoftphoneParam
    class SoftphoneParam
    {
        public bool RegistrationRequired { get; set; }
        public string DisplayName { get; set; }
        public string UserName { get; set; }
        public string AuthenticationId { get; set; }
        public string RegisterPassword { get; set; }
        public string DomainHost { get; set; }
        public int DomainPort { get; set; }

        public int MaxConcurrentCall { get; set; }
    }

    // class SoftphoneID
    class SoftphoneID
    {
        static int _LastID = 1;
        public int ID { get; private set; }


        public SoftphoneID()
        {
            ID = _LastID++;
        }
    }

    class PlayMP3
    {
        MediaConnector _connector;
        MP3StreamPlayback _MP3file;
        PhoneCallAudioSender _mediaSender;
        IPhoneCall _Call;


        public PlayMP3(string Mp3FileName, IPhoneCall Call)
        {
            _connector = new MediaConnector();
            _MP3file = new MP3StreamPlayback(Mp3FileName);
            _mediaSender = new PhoneCallAudioSender();
            _Call = Call;
        }

        public void Start()
        {
            _mediaSender.AttachToCall(_Call);
            _connector.Connect(_MP3file, _mediaSender);
            _MP3file.Start();
        }

        public void Stop()
        {
            _MP3file.Stop();
        }

        public void Dispose()
        {
            _MP3file.Dispose();
        }
    }

    // class Softphone
    class Softphone
    {
        ISoftPhone _softphone;
        IPhoneLine _phoneLine;
        int _currentConcurrentCall;
        CallHandler _callHandler;
        static int _LastSoftphoneNumber = 1;
        PlayMP3 _MP3;
        SoftphoneParam _softphoneParam;

        SoftphoneID _softphoneID;
        myQueue _softphoneQueue;
        public bool RegistrationSucceeded { set; get; }

        public Softphone(SoftphoneParam softphoneParam)
        {
            _softphone = SoftPhoneFactory.CreateSoftPhone(5000, 10000);
            _LastSoftphoneNumber++;
            _softphoneParam = softphoneParam;
            _softphoneID = new SoftphoneID();
            _softphoneQueue = new myQueue();
            RegistrationSucceeded = false;
        }

        public void StartPlayFile(string fileName)
        {
            if (_MP3 != null)
            {
                _MP3.Dispose();
            }
            _MP3 = new PlayMP3(fileName, _callHandler.PhoneCall());
            _MP3.Start();
        }

        public PlayMP3 MP3()
        {
            return _MP3;
        }

        public int GetCurrentConcurrentCalls()
        {
            return _currentConcurrentCall;
        }

        public void IncCurrentConcurrentCalls()
        {
            _currentConcurrentCall++;
        }

        public void DecCurrentConcurrentCalls()
        {
            _currentConcurrentCall--;
        }

        public SoftphoneID SoftphoneID()
        {
            return _softphoneID;
        }

        public BlockingQueue<DialerEvent> SoftphoneQueue()
        {
            return _softphoneQueue;
        }

        public IPhoneCall PhoneCall()
        {
            if (_callHandler != null)
            {
                return _callHandler.PhoneCall();
            }

            return null;
        }

        public void Register()
        {
            try
            {
             
                var account = new SIPAccount(_softphoneParam.RegistrationRequired, _softphoneParam.DisplayName, _softphoneParam.UserName,
                                            _softphoneParam.AuthenticationId, _softphoneParam.RegisterPassword, _softphoneParam.DomainHost,
                                            _softphoneParam.DomainPort);
                Console.WriteLine("\nCreating SIP account {0}", account);

                _softphone.IncomingCall += softphone_IncomingCall;
                _phoneLine = _softphone.CreatePhoneLine(account);
                Console.WriteLine("Phoneline created.");

                _phoneLine.RegistrationStateChanged += phoneLine_RegistrationStateChanged;

                _softphone.RegisterPhoneLine(_phoneLine);

            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during SIP registration: {0}", ex.ToString());
            }
        }

        private void softphone_IncomingCall(object sender, VoIPEventArgs<IPhoneCall> e)
        {        
            Console.WriteLine("Incoming, SF {0} ", _softphoneID.ID);
            _callHandler = new CallHandler(this,e.Item);
            DialerEvent dialerEvent = new DialerEvent(eEventType.Incoming);
            _softphoneQueue.Enqueue(dialerEvent);       
            
        }

        private void phoneLine_RegistrationStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            var handler = PhoneLineStateChanged;
            if (handler != null)
                handler(this, e);
        }

        public event EventHandler<RegistrationStateChangedArgs> PhoneLineStateChanged;

        public IPhoneCall CreateCall(string member)
        {
            return _softphone.CreateCallObject(_phoneLine, member);
        }

        public CallID StartCall(string callToNumber)
        {
            _callHandler = new CallHandler(this);
            _callHandler.Dial(callToNumber);

            return _callHandler.CallID();
        }
    }

    // class
    class Softphones : List<Softphone>
    {
        static object _sync;

        public Softphones()
        {
            _sync = new object();
        }

        public Softphone GetAvailable()
        {
            for (int j = 0; j < 1000; j++)
            {
                for (int i = 0; i < this.Count; i++)
                {
                    lock (_sync)
                    {
                        if (this[i].GetCurrentConcurrentCalls() == 0 && this[i].RegistrationSucceeded == true)
                        {

                            this[i].IncCurrentConcurrentCalls();
                            return this[i];
                        }
                    }
                }
                Thread.Sleep(2000);
            }
            // Error, no available softphone for long time
            return null;
        }

        public void FreeSoftphone(Softphone softphone)
        {
            lock (_sync)
            {
                softphone.DecCurrentConcurrentCalls();
            }
        }
    }
}
