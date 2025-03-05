using Confluent.Kafka;
using System;
using System.ServiceModel;
using System.Threading.Tasks;
using TicimaxAPI.Models;
using TicimaxAPI.WcfServices;
using TicimaxAPI.WcfServices.Custom;
using TicimaxAPI.WcfServices.Siparis;
using TicimaxAPI.WcfServices.Urun;
using TicimaxAPI.WcfServices.Uye;

namespace TicimaxAPI.Services
{
    public class TicimaxWcfClient
    {
        private readonly string _uyeKodu;
        private readonly string _siteName;
        private readonly string _baseUrl;

        public TicimaxWcfClient(string uyeKodu, string siteName, string baseUrl)
        {
            _uyeKodu = uyeKodu;
            _siteName = siteName;
            _baseUrl = baseUrl;
        }
        private BasicHttpBinding CreateBasicHttpBinding()
        {
            return new BasicHttpBinding(BasicHttpSecurityMode.Transport)
            {
                MaxBufferSize = int.MaxValue,
                MaxReceivedMessageSize = int.MaxValue,
                ReaderQuotas = System.Xml.XmlDictionaryReaderQuotas.Max
            };
        }
        public async Task<object> HandleActionAsync(TicimaxRequestDto request)
        {
            switch (request.ActionType)
            {
                case "add_to_cart":
                case "remove_to_cart":
                    return await GetSepet(request.CustomerId);
                case "add_favorite_product":
                    return await GetFavoriUrunler(request.CustomerId);
                case "checkout":
                    return await GetSiparis(request.CustomerId);
                default:
                    throw new ArgumentException("Geçersiz ActionType");
            }
        }

        private async Task<object> GetSepet(string customerId)
        {
            var binding = CreateBasicHttpBinding();
            var endpoint = new EndpointAddress($"{_baseUrl}/Servis/SiparisServis.svc");

            using (var client = new WcfServices.Siparis.SiparisServisClient(binding, endpoint))
            {
                var request = new SelectWebSepetRequest
                {
                    Dil =  "TR",
                    ParaBirimi = "TL",
                    SepetId = 0,
                    UyeId = int.Parse(customerId)
                };

                return await client.SelectWebSepetAsync(_uyeKodu, request);
            }
        }

        private async Task<object> GetFavoriUrunler(string customerId)
        {
            var binding = CreateBasicHttpBinding();
            var endpoint = new EndpointAddress($"{_baseUrl}/Servis/CustomServis.svc");

            using (var client = new WcfServices.Custom.CustomServisClient(binding, endpoint))
            {
                var request = new GetFavoriUrunlerRequest
                {
                    UyeID = int.Parse(customerId)
                };

                return await client.GetFavoriUrunlerAsync(_uyeKodu, request);
            }
        }

        private async Task<object> GetSiparis(string customerId)
        {
            var binding = CreateBasicHttpBinding();
            var endpoint = new EndpointAddress($"{_baseUrl}/Servis/SiparisServis.svc");

            using (var client = new WcfServices.Siparis.SiparisServisClient(binding, endpoint))
            {
                var request = new WebSiparisFiltre
                {
                    UyeID = int.Parse(customerId)
                };

                var sayfalama = new WebSiparisSayfalama
                {
                    BaslangicIndex = 0,
                    KayitSayisi = 100
                };

                return await client.SelectSiparisAsync(_uyeKodu, request, sayfalama);
            }
        }

        public async Task<object> GetCustomerData(string customerId)
        {
            var binding = CreateBasicHttpBinding();
            var endpoint = new EndpointAddress($"{_baseUrl}/Servis/UyeServis.svc");

            using (var client = new WcfServices.Uye.UyeServisClient(binding, endpoint))
            {
                var request = new UyeFiltre
                {
                    Aktif = -1,
                    AlisverisYapti = -1,
                    Cinsiyet = -1,
                    MailIzin = -1,
                    SmsIzin = -1,
                    UyeID = int.Parse(customerId)
                };

                var sayfalama = new UyeSayfalama
                {
                    //KayitSayisi = 1
                };
                var customerData = await client.SelectUyelerAsync(
                    _uyeKodu, 
                    request, 
                    sayfalama);

                return customerData;
            }
        }
    }
}
