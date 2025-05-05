using System;

namespace InvoiceManager.Models
{
    public class AboutViewModel
    {
        readonly string _ageInPolish;

        public AboutViewModel()
        {
            int now = int.Parse(DateTime.Now.ToString("yyyyMMdd"));
            int birthDate = 20000210; //yyyyMMdd
            byte age = (byte)((now - birthDate) / 10000);
            _ageInPolish = (age % 10 > 1 && age % 10 < 5) ? age + " lata" : age + " lat";
        }

        public string Title => "O mnie";
        public string Description => @$"Mam na imię Marek i mam {_ageInPolish}.
                                     Jestem przedstawicielem firmy Xenial Programmer sp. z o.o. specjalizującej się w programowaniu.

                                     Moją firmę cechuje:
                                     💡 Doświadczenie – 10 lat na rynku, tysiące zadowolonych klientów;
                                     🚀 Innowacje – Stawiam na nowoczesne technologie i przełomowe rozwiązania;
                                     🤝 Partnerstwo – Tworzę rozwiązania dopasowane do Twoich potrzeb.";
    }
}