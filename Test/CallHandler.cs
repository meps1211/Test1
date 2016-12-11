using System;
using Ozeki.VoIP;
using Ozeki.Media;

namespace DialerNS
{
    enum eEventType
    {
        Ring,
        Answer,
        End,
        DTMF1,
        DTMF2,
        DTMF3,
        WrongDTMF,
        TimeOut,
        Incoming
    }

    
    // DialerEvent

    /// <summary>
    /// 
    /// </summary>
    class DialerEvent
    {
        public eEventType EventType { private set; get; }
        public DialerEvent(eEventType Event)
        {
            EventType = Event;
        }
    }

    // class CallID
    class CallID
    {
        static int _LastID = 1;
        int _ID;


        public CallID()
        {
            _ID = _LastID++;
        }

        public int ID
        {
            get { return _ID; }
        }
    }

    // class Script
    class Script
    {
        public bool Answer { get; set; }
        public int WaitAfterAnswer { get; set; }
        public eEventType PressDTMF { get; set; }
      
    }

    // class CallHandler
    class CallHandler
    {
        Softphone _softphone;
        IPhoneCall _call;
        CallID _callID;

        public CallHandler(Softphone softphone)
        {
            _softphone = softphone;
            _callID = new CallID();
        }

        public CallHandler(Softphone softphone, IPhoneCall call)
        {
            _call = call;
            _softphone = softphone;
            _callID = new CallID();
        }

        public IPhoneCall PhoneCall()
        {
            return _call;
        }

        public CallID CallID()
        {
            return _callID;
        }

        //public event EventHandler Completed;

        public void Dial(string callToNumber)
        {
            _call = _softphone.CreateCall(callToNumber);
            _call.CallStateChanged += OutgoingCallStateChanged;
            _call.Start();
            _call.DtmfReceived += DtmfReceived;
            Console.WriteLine("Trying to call: {0}.", callToNumber);
        }

        private void Enqueue(eEventType EventType)
        {
            DialerEvent dialerEvent = new DialerEvent(EventType);         
            _softphone.SoftphoneQueue().Enqueue(dialerEvent);
        }

        private void OutgoingCallStateChanged(object sender, CallStateChangedArgs e)
        {
            if (e.State == CallState.Answered)
            {
                Enqueue(eEventType.Answer);
            }
            else if (e.State.IsCallEnded())
            {
                Enqueue(eEventType.End);
            }
        }

        void DtmfReceived(object sender, VoIPEventArgs<DtmfInfo> e)
        {
            int Dtmf = e.Item.Signal.Signal;
            switch (Dtmf.ToString())
            {
                case "1":                   
                        Enqueue(eEventType.DTMF1);
                        break;
                    
                case "2":                    
                        Enqueue(eEventType.DTMF2);
                        break;                    
                case "3":                    
                        Enqueue(eEventType.DTMF3);
                        break;
                default:
                        Enqueue(eEventType.WrongDTMF);                    
                        break;
            }
           
        }
    }
}
