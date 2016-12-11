using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Ozeki.VoIP;

namespace DialerNS
{
    // class Program
    class Program
    {

        static void Main(string[] args)
        {
            Ozeki.Common.LicenseManager.Instance.SetLicense("OZSDK-TAL32CALL-140520-EE768FF1", "TUNDOjMyLE1QTDozMixHNzI5OnRydWUsTVNMQzozMixNRkM6MzIsVVA6MjAxNS4wNS4yMCxQOjIxOTkuMDEuMDF8d2VqU2lEWmFxTGpENmNsZ0s1ZEdOTUwxRkRtMGwxckdObkRIcHk2Z1hQKzAwMjVzSDduSUpZL0s0U1BxOHVIclZleTlZckNFZGp2RzBQK29ZZGtxSFE9PQ==");

            //string DomainHost = "10.20.2.73";
            string DomainHost = "192.168.1.35";

            Dialer dialer = new Dialer();

            int numberArg = args.Length;

            string arg = args[0].ToString();
            Console.WriteLine("arg {0}", arg);

            SoftphoneParam softphoneParamWork = new SoftphoneParam();
            softphoneParamWork.RegistrationRequired = true;
            softphoneParamWork.DisplayName = "miki" + arg;
            softphoneParamWork.UserName = "222" + arg;
            Console.WriteLine("Start Ext = {0}", softphoneParamWork.UserName);
            softphoneParamWork.AuthenticationId = "miki" + arg;
            softphoneParamWork.RegisterPassword = "12345";
            softphoneParamWork.DomainHost = DomainHost;
            softphoneParamWork.DomainPort = 5060;
            softphoneParamWork.MaxConcurrentCall = 1;

            Softphone softphone = dialer.CreateSoftphone(softphoneParamWork);
            Console.WriteLine("Extension: " + "222" + arg);


            Script script = new Script();
            Random random = new Random();
            int randomNum = random.Next(0, 10000);
            script.WaitAfterAnswer = randomNum;
            script.Answer = (randomNum < (10000 * 0.9));
            int randomDTMF = random.Next(0, 4);
            switch (randomDTMF)
            {
                case 0:
                    script.PressDTMF = eEventType.DTMF1;
                    break;
                case 1:
                     script.PressDTMF = eEventType.DTMF2;
                    break;
                case 2:
                    script.PressDTMF = eEventType.DTMF3;
                    break;
                default:
                    script.PressDTMF = eEventType.WrongDTMF;
                    break;
            }
           
            dialer.RunScript(script, softphone);

        }

    }
}
