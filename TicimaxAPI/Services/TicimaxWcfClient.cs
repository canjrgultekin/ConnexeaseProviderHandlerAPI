using System;
using System.ServiceModel;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TicimaxAPI.Models;
using TicimaxAPI.WcfServices;
using TicimaxAPI.WcfServices.Custom;
using TicimaxAPI.WcfServices.Siparis;
using TicimaxAPI.WcfServices.Urun;
using TicimaxAPI.WcfServices.Uye;
using TicimaxAPI.Helper;

namespace TicimaxAPI.Services
{
    public class TicimaxWcfClient
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TicimaxWcfClient> _logger;

        public TicimaxWcfClient(IConfiguration configuration, ILogger<TicimaxWcfClient> logger)
        {
            _configuration = configuration;
            _logger = logger;
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

        public async Task<object> GetSepet(TicimaxFirmConfig firmaConfig, string customerId)
        {
            var binding = CreateBasicHttpBinding();
            var endpoint = new EndpointAddress($"{firmaConfig.BaseUrl}/Servis/SiparisServis.svc");

            using (var client = new WcfServices.Siparis.SiparisServisClient(binding, endpoint))
            {
                var request = new SelectWebSepetRequest
                {
                    Dil = "TR",
                    ParaBirimi = "TL",
                    SepetId = 0,
                    UyeId = int.Parse(customerId)
                };

                return await client.SelectWebSepetAsync(firmaConfig.UyeKodu, request);
            }
        }

        public async Task<object> GetFavoriUrunler(TicimaxFirmConfig firmaConfig, string customerId)
        {
            var binding = CreateBasicHttpBinding();
            var endpoint = new EndpointAddress($"{firmaConfig.BaseUrl}/Servis/CustomServis.svc");

            using (var client = new WcfServices.Custom.CustomServisClient(binding, endpoint))
            {
                var request = new GetFavoriUrunlerRequest
                {
                    UyeID = int.Parse(customerId)
                };

                return await client.GetFavoriUrunlerAsync(firmaConfig.UyeKodu, request);
            }
        }

        public async Task<object> GetSiparis(TicimaxFirmConfig firmaConfig, string customerId)
        {
            var binding = CreateBasicHttpBinding();
            var endpoint = new EndpointAddress($"{firmaConfig.BaseUrl}/Servis/SiparisServis.svc");

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

                return await client.SelectSiparisAsync(firmaConfig.UyeKodu, request, sayfalama);
            }
        }

        public async Task<object> GetCustomerData(TicimaxFirmConfig firmaConfig, string customerId)
        {
            var binding = CreateBasicHttpBinding();
            var endpoint = new EndpointAddress($"{firmaConfig.BaseUrl}/Servis/UyeServis.svc");

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
                var customerData = await client.SelectUyelerAsync(firmaConfig.UyeKodu, request, sayfalama);

                return customerData;
            }
        }
    }
}
