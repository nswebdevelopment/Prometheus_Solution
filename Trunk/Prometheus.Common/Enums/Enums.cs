namespace Prometheus.Common.Enums
{
    public enum StatusEnum
    {
        Success,
        NotFound,
        Error
    }

    public enum Statuses
    {
        Active = 1,
        Deleted = 2,
        Suspended = 3,
        Unnotified = 4,
        Notified = 5
    }

    public enum JobStatus
    {
        Draft = 1,
        Pending = 2,
        Executing = 3,
        Done = 4,
        Failed = 5
    }

    public enum TxStatus
    {
        Fail = 0,
        Success = 1,
        Pending = 2,
        None = 3
    }

    public enum TransactionStatus
    {
        Success = 1,
        Pending = 2,
        Failed = 3
    }

    public enum AvailableExchanges
    {
        Binance = 1,
        Bitfinex = 2,
        Bitstamp = 3,
        Gdax = 4,
        Gemini = 5,
        Kraken = 6,
        Okex = 7,
        Poloniex = 8
    }

    public enum Cryptocurrency
    {
        Bitcoin,
        Ethereum,
        Ripple,
        Litecoin,
        EOS,
        NEO,
        Cardano,
        Stellar
    }

    public enum Statement
    {
        Credit,
        Debit
    }

    public enum AdapterType
    {
        Enterprise = 1,
        Crypto = 2,
        Business = 3
    }
    public enum AdapterTypeItemEnum
    {
        MSSQL = 1,
        MySQL = 2,
        Oracle = 3,
        MongoDB = 4,
        Ethereum = 5,
        Cardano = 6,
        EOS = 7,
        NEO = 8,
        Bitcoin = 9,
        Excel = 10,
        MATLAB = 11,
        Litecoin = 12
    }


    public enum EnterpriseAdapterType
    {
        MSSQL = 1,
        MySQL = 2,
        Oracle = 3,
        MongoDB = 4
    }


    public enum CryptoAdapterType
    {
        Ethereum = 5,
        Cardano = 6,
        EOS = 7,
        NEO = 8,
        Bitcoin = 9,
        Litecoin = 12
    }

    public enum BusinessAdapterType
    {
        Excel = 10,
        MATLAB = 11
    }
    public enum DataTypes
    {
        numeric = 1,
        text = 2,
        datetime = 3,
        bit = 4
    }

    public enum PropertyName
    {
        TransactionId = 1,
        TransactionAccount = 2,
        TransactionAmount = 3,
        TransactionType = 4
    }

    public enum DirectionEnum
    {
        Source = 1,
        Destination = 2
    }

    public enum PropertyTypeEnum
    {
        Text = 1,
        Numeric = 2,
        DateTime = 3,
        Boolean = 4,
        Password = 5
    }

    public enum PropertyEnum
    {
        CollectionName = 1,
        RpcUsername = 2,
        RpcPassword = 3,
        Coinbase = 4,
        CoinbasePassword = 5,
        NewAccountPassword = 6,
        FromBlock = 7,
        ToBlock = 8,
        EthereumAccount = 9,
        TableNamePrefix = 10,
        BitcoinAddress = 11,
        NeoAddress = 12,
        LitecoinAddress = 13
    }

    public enum InsertQueryType
    {
        Blocks = 1,
        Transactions = 2,
        TransactionInputs = 3,
        TransactionOutputs = 4
    }
}


