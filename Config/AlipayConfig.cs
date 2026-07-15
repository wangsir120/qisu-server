namespace qisu_server.Config
{
    public class QisuAlipayConfig
    {
        public string AppId { get; set; } = "9021000163603817";
        public string PrivateKey { get; set; } = "MIIEvgIBADANBgkqhkiG9w0BAQEFAASCBKgwggSkAgEAAoIBAQCRNYAB5CQB6sZTLUXIELR7S8vHm291TIQ9hiVtaujagadNUv9SG9osD0q2OTkPTxyEskyNp3SiFA1/4K4byUPNxFiwx/zvOiN114VblCA1fxrx35sv/b7tgqL2ES7YaRQl0Xibj7hlrcLF40XKYeIpnwO6xsuzp3O3OYVVnELXjP1HVelge7lj87jlYuP7RrTIgGncgv4CKvtMNiTLgog0TzdWSk412/NOLMz64eimWG2KXFLzWLRr1Fds4emcNoWYFuWdVgFbSLsjX97kRK1gnz/yYAZCfcU338wo21TIuV1aOOtlPuVPbFnoyw9ba8rg08h4QfLeInTb8NNu12SzAgMBAAECggEARwDwHPyflshkPdvPWWrBteB0PqCEuci2iRcFSiGSxvXLBwZkjpPL9OttTvlgK1o1ybUdtc6CO5aumy8UM8YQf5dY/uhrh9bX7BF8xjECJuaGGGuMiMT9DUppwQTZ8TxAe9WXsglu01lJ+lWNlM6UNmHpAvWeObTR4nAgAKKuFJO+yYvJ22tvbROTOu5UTSa1P4NbrLCO5CCeNJU2AbGvx8t/71U/8NgGMZCeutWz4URJ5i1r4RhRFqtWml4q2IErUgcz3m3vXF8pzG1fMrWuThbgBAuuRFf8vYfblIcRsawGDsSaXBUATjEiXkTUT9g6egDvsMLemmllhTDwgevZoQKBgQDm+enKxHDUL9JgYttFcMBJB2M+h2ccv7kVquI7poEmiA60D4v4XUWKi5BUNJcaliJyNofF1xjtmUFNmYpnz8SA5QXLoLs4mhkLxiJsXl/7m49xw8JRvjJRkGDqSHG4kUPvPjU8VfPjkgPMe3+4FnNNo20otO1YmaPkrK9mx4J7CwKBgQCg8NjIvUOz8T4N1EWTjZWo6aSQrKUshZmtWXe+FEenGOK2u1GFhVDF5MN8PvaZqrkxGjj3ZsOykmAPvUmQu1V4AxuJC8jMuv+8kIRXJ1mnd2doGPQfVspE0D+OdRsIjQzIX5Nzv11w8kv/PHFkv6drT8+kwBESoNd1wP6QHaOF+QKBgQDXa3BMJ8hfbbaVJL4C1rTPp689C0X0/y8c8UKMha9gg3bLItDVtA/+tknG70GajznTMd6RexqJxuyr9i6qwZEw8ejk0KSslrQTUhia364/WQeBACXE3VHK1pA9EZHWpM0qXeeCvVt4/J7EYM5un6msWGafl5bhknHT/eadQigEnwKBgGs254QbKZ4XSRqXXd5lRN0pAPtsOAEH44+q+W1EP1Oe7XGEKlPDs0KSGnSL0WYfaI3AhVCzQg2VG6+AjyB+2/o+P7q1ggh5TuLEd5VD3qMElXuwm/jArbDX9m8lrmOs62YU3bsPMeWLVHttPbE7SAHiQlbjqLv7MG3+qtdBF22pAoGBAMoGX/UZGb/537pVhZfTnYa+Y4iRW8b5ZpVkxcctM9saonQGLRESvFNLZh9WfXfg64d2/t9J8bJy6XI4lXtzIZQJjZgMDwzJnO2VKiCSracqxzAkA3KW5ccCFwVhKjcnx0PmHWSl8yJK0S1pQzAe4CbhG1BVPxRzg4ErCIo4vN1G";
        public string AlipayPublicKey { get; set; } = "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAkTWAAeQkAerGUy1FyBC0e0vLx5tvdUyEPYYlbWro2oGnTVL/UhvaLA9Ktjk5D08chLJMjad0ohQNf+CuG8lDzcRYsMf87zojddeFW5QgNX8a8d+bL/2+7YKi9hEu2GkUJdF4m4+4Za3CxeNFymHiKZ8DusbLs6dztzmFVZxC14z9R1XpYHu5Y/O45WLj+0a0yIBp3IL+Air7TDYky4KINE83VkpONdvzTizM+uHoplhtilxS81i0a9RXbOHpnDaFmBblnVYBW0i7I1/e5EStYJ8/8mAGQn3FN9/MKNtUyLldWjjrZT7lT2xZ6MsPW2vK4NPIeEHy3iJ02/DTbtdkswIDAQAB";
        public string GatewayUrl { get; set; } = "https://openapi-sandbox.dl.alipaydev.com/gateway.do";
        public string NotifyUrl { get; set; } = "http://b6992bed.natappfree.cc/api/payment/notify";
        public string ReturnUrl { get; set; } = "http://localhost:5754/qixu-web/client/index/booking/success";
        public string Charset { get; set; } = "UTF-8";
        public string SignType { get; set; } = "RSA2";
        public string Format { get; set; } = "json";
        public string Version { get; set; } = "1.0";
    }
}
