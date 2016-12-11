using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ozeki.VoIP;
using Ozeki.Media;

namespace DialerNS
{


    // class Dialer
    class Dialer
    {
        static Softphones _mySoftphones;

        public Dialer()
        {
            _mySoftphones = new Softphones();
        }

        public Softphone CreateSoftphone(SoftphoneParam softphoneParam)
        {
            Softphone softphone = new Softphone(softphoneParam);
            _mySoftphones.Add(softphone);
            int Last = _mySoftphones.Count - 1;
            _mySoftphones[Last].PhoneLineStateChanged += mySoftphone_PhoneLineStateChanged;
            _mySoftphones[Last].Register();

            return softphone;
        }


        public void RunScript(Script script, Softphone softphone)
        {       
            DialerEvent dialerEvent;           
            Console.WriteLine("SF {0}", softphone.SoftphoneID().ID);
            softphone.SoftphoneQueue().Clear();

            while (true)
            {
                softphone.SoftphoneQueue().TryDequeue(out dialerEvent, 80000);
                if (dialerEvent.EventType == eEventType.Incoming)
                {
                    if (script.Answer == true)
                    {
                        softphone.PhoneCall().Answer();
                        Thread.Sleep(script.WaitAfterAnswer);
                        DtmfNamedEvents SendDTMF;
                        switch (script.PressDTMF)
                        {
                            case eEventType.DTMF1:
                                SendDTMF = DtmfNamedEvents.Dtmf1;
                                break;
                            case eEventType.DTMF2:
                                SendDTMF = DtmfNamedEvents.Dtmf2;
                                break;
                            case eEventType.DTMF3:
                                SendDTMF = DtmfNamedEvents.Dtmf3;
                                break;
                            default:
                                SendDTMF = DtmfNamedEvents.Dtmf4;
                                break;
                        }
                        softphone.PhoneCall().StartDTMFSignal(SendDTMF, DtmfSignalingMode.SIPINFO);                        
                    }
                }
                //Ended(softphone);
            }
  
        }


        private void Ended(Softphone softphone)
        {           
            IPhoneCall call = softphone.PhoneCall();
            if (call != null)
            {
                call.HangUp();
            }
            _mySoftphones.FreeSoftphone(softphone);
        }

        static void mySoftphone_PhoneLineStateChanged(object sender, RegistrationStateChangedArgs e)
        {
            Console.WriteLine("Phone line state changed: {0}", e.State);
            Softphone softphone = sender as Softphone;

            if (e.State == RegState.Error || e.State == RegState.NotRegistered)
                softphone.Register();

            if (e.State == RegState.RegistrationSucceeded)
            {
                softphone.RegistrationSucceeded = true;
                Console.WriteLine("Registration succeeded - Online!\n");
            }
        }

    }
}
