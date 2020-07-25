using Prometheus.Model.Models;
using Prometheus.Model.Models.BitcoinAdapterModel;
using System;
using System.Collections.Generic;
using System.Linq;
using Prometheus.Common.Enums;
using System.Text;
using Prometheus.Model.Models.NeoAdapterModel;
using Prometheus.Model.Models.LitecoinBlockModel;

namespace Prometheus.DbAdapter.Queries
{
    public class QueryWrite
    {
        public string TableNamePrefix { get; set; }
        public CryptoAdapterType CryptoAdapterType { get; set; }

        public List<EthereumBlockModel> EthereumBlockModel { get; set; }
        public List<BitcoinBlockModel> BitcoinBlockModel { get; set; }
        public List<NeoBlockModel> NEOBlockModel { get; set; }
        public List<LitecoinBlockModel> LitecoinBlockModel { get; set; }

        
        #region Create
        #region MySQL
        public string CreateTableQueryMySQL
        {
            get
            {
                switch (CryptoAdapterType)
                {
                    case CryptoAdapterType.Ethereum:
                        return $@"CREATE TABLE {TableNamePrefix}EthereumBlock (
                                    Id nvarchar(200) NOT NULL, 
                                    BlockHeight bigint NOT NULL, 
                                    TimeStamp datetime NOT NULL, 
                                    PRIMARY KEY (Id)
                                );

                           CREATE TABLE {TableNamePrefix}EthereumTransaction(
                                Id nvarchar(200) NOT NULL, 
                                TxHash nvarchar(200) NOT NULL, 
                                FromAccount nvarchar(500) NOT NULL, 
                                ToAccount nvarchar(500) NULL, 
                                Value decimal(16, 6) NOT NULL, 
                                TxReceiptStatus nvarchar(20) NULL, 
                                BlockId nvarchar(200) NOT NULL, 
                                PRIMARY KEY(Id), 
                                FOREIGN KEY (BlockId) REFERENCES {TableNamePrefix}EthereumBlock(Id)
                            );";

                    case CryptoAdapterType.Bitcoin:
                    case CryptoAdapterType.Litecoin:

                        return $@"CREATE TABLE {TableNamePrefix}{CryptoAdapterType.ToString()}Block (
                                Id nvarchar(200) NOT NULL, 
                                BlockHeight bigint NOT NULL, 
                                TimeStamp datetime NOT NULL, 
                                PRIMARY KEY (Id)
                            );

                           CREATE TABLE {TableNamePrefix}{CryptoAdapterType.ToString()}Transaction(
                                Id nvarchar(200) NOT NULL, 
                                TxHash nvarchar(200) NOT NULL, 
                                TotalOutValue decimal(16, 6) NOT NULL, 
                                BlockId nvarchar(200) NOT NULL, 
                                PRIMARY KEY(Id), 
                                FOREIGN KEY (BlockId) REFERENCES {TableNamePrefix}{CryptoAdapterType.ToString()}Block(Id)
                            );

                            CREATE TABLE {TableNamePrefix}{CryptoAdapterType.ToString()}TransactionInput(
                                Id nvarchar(200) NOT NULL, 
                                Address nvarchar(500) NOT NULL, 
                                TransactionId nvarchar(200) NOT NULL, 
                                PRIMARY KEY(Id), 
                                FOREIGN KEY (TransactionId) REFERENCES {TableNamePrefix}{CryptoAdapterType.ToString()}Transaction(Id)
                            );

                            CREATE TABLE {TableNamePrefix}{CryptoAdapterType.ToString()}TransactionOutput(
                                Id nvarchar(200) NOT NULL, 
                                Address nvarchar(500) NULL, 
                                Value decimal(16, 6) NOT NULL, 
                                TransactionId nvarchar(200) NOT NULL, 
                                PRIMARY KEY(Id), 
                                FOREIGN KEY (TransactionId) REFERENCES {TableNamePrefix}{CryptoAdapterType.ToString()}Transaction(Id)
                            );";
                    case CryptoAdapterType.NEO:

                        return $@"CREATE TABLE {TableNamePrefix}NEOBlock (
                                Id nvarchar(200) NOT NULL, 
                                BlockIndex bigint NOT NULL, 
                                TimeStamp datetime NOT NULL, 
                                PRIMARY KEY (Id)
                            );

                           CREATE TABLE {TableNamePrefix}NEOTransaction(
                                Id nvarchar(200) NOT NULL, 
                                TxId nvarchar(200) NOT NULL, 
                                TxType nvarchar(200) NOT NULL, 
                                BlockId nvarchar(200) NOT NULL, 
                                PRIMARY KEY(Id), 
                                FOREIGN KEY (BlockId) REFERENCES {TableNamePrefix}NEOBlock(Id)
                            );

                            CREATE TABLE {TableNamePrefix}NEOTransactionInput(
                                Id nvarchar(200) NOT NULL, 
                                TxInId nvarchar(200) NOT NULL, 
                                TransactionId nvarchar(200) NOT NULL, 
                                PRIMARY KEY(Id), 
                                FOREIGN KEY (TransactionId) REFERENCES {TableNamePrefix}NEOTransaction(Id)
                            );

                            CREATE TABLE {TableNamePrefix}NEOTransactionOutput(
                                Id nvarchar(200) NOT NULL, 
                                Address nvarchar(500) NOT NULL, 
                                Asset nvarchar(200) NOT NULL,
                                Value decimal(16, 6) NOT NULL,
                                TransactionId nvarchar(200) NOT NULL, 
                                PRIMARY KEY(Id), 
                                FOREIGN KEY (TransactionId) REFERENCES {TableNamePrefix}NEOTransaction(Id)
                            );";
                    default:
                        return String.Empty;
                }
            }
        }
        #endregion

        #region Oracle
        public string CreateTableQueryOracle
        {
            get
            {
                switch (CryptoAdapterType)
                {
                    case CryptoAdapterType.Ethereum:
                        return $@"BEGIN 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_ETHEREUM_BLOCK ( 
                                    ID varchar2(200)NOT NULL, 
                                    BLOCK_HEIGHT number NOT NULL, 
                                    TIMESTAMP timestamp NOT NULL, 
                                    PRIMARY KEY (ID)
                                )'; 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_ETHEREUM_TRANSACTION ( 
                                    ID varchar2(200) NOT NULL, 
                                    TX_HASH varchar2(200) NOT NULL, 
                                    FROM_ACCOUNT varchar2(500) NOT NULL, 
                                    TO_ACCOUNT varchar2(500) NULL, 
                                    VALUE number(16,6) NOT NULL, 
                                    TX_RECEIPT_STATUS varchar2(20) NULL, 
                                    BLOCK_ID varchar2(200) NOT NULL, 
                                    PRIMARY KEY (ID), 
                                    CONSTRAINT FK_{TableNamePrefix}_ETHEREUM_BLOCK_TRANSACTION FOREIGN KEY(BLOCK_ID) 
                                    REFERENCES {TableNamePrefix}_ETHEREUM_BLOCK(ID)
                                )'; 
                                END;";

                    case CryptoAdapterType.Bitcoin:
                    case CryptoAdapterType.Litecoin:
                        
                        return $@"BEGIN 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_BLOCK ( 
                                    ID varchar2(200) NOT NULL, 
                                    BLOCK_HEIGHT number NOT NULL, 
                                    TIMESTAMP timestamp NOT NULL, 
                                    PRIMARY KEY (ID)
                                )'; 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_TRANSACTION ( 
                                    ID varchar2(200) NOT NULL, 
                                    TX_HASH varchar2(200) NOT NULL, 
                                    TOTAL_OUT_VALUE number(16,6) NOT NULL, 
                                    BLOCK_ID varchar2(200) NOT NULL, 
                                    PRIMARY KEY (ID), 
                                    CONSTRAINT FK_{TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_BLOCK_TRANSACTION FOREIGN KEY(BLOCK_ID) 
                                    REFERENCES {TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_BLOCK(ID)
                                )'; 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_TRANSACTION_INPUT ( 
                                    ID varchar2(200) NOT NULL, 
                                    ADDRESS varchar2(500) NOT NULL, 
                                    TRANSACTION_ID varchar2(200) NOT NULL, 
                                    PRIMARY KEY (ID), 
                                    CONSTRAINT FK_{TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_BLOCK_TRANSACTION_INPUT FOREIGN KEY(TRANSACTION_ID) 
                                    REFERENCES {TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_TRANSACTION(ID)
                                )'; 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_TRANSACTION_OUTPUT (
                                    ID varchar2(200) NOT NULL, 
                                    ADDRESS varchar2(500) NULL, 
                                    VALUE number(16,6) NOT NULL, 
                                    TRANSACTION_ID varchar2(200) NOT NULL, 
                                    PRIMARY KEY (ID), 
                                    CONSTRAINT FK_{TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_BLOCK_TRANSACTION_OUTPUT FOREIGN KEY(TRANSACTION_ID) 
                                    REFERENCES {TableNamePrefix}_{CryptoAdapterType.ToString().ToUpper()}_TRANSACTION(ID)
                                )'; 
                                END;";
                    case CryptoAdapterType.NEO:

                        return $@"BEGIN 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_NEO_BLOCK ( 
                                    ID varchar2(200) NOT NULL, 
                                    BLOCK_INDEX number NOT NULL, 
                                    TIMESTAMP timestamp NOT NULL, 
                                    PRIMARY KEY (ID)
                                )'; 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_NEO_TRANSACTION ( 
                                    ID varchar2(200) NOT NULL, 
                                    TX_ID varchar2(200) NOT NULL, 
                                    TX_TYPE varchar2(200) NOT NULL, 
                                    BLOCK_ID varchar2(200) NOT NULL, 
                                    PRIMARY KEY (ID), 
                                    CONSTRAINT FK_{TableNamePrefix}_NEO_BLOCK_TRANSACTION FOREIGN KEY(BLOCK_ID) 
                                    REFERENCES {TableNamePrefix}_NEO_BLOCK(ID)
                                )'; 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_NEO_TRANSACTION_INPUT ( 
                                    ID varchar2(200) NOT NULL, 
                                    TX_IN_ID varchar2(500) NOT NULL, 
                                    TRANSACTION_ID varchar2(200) NOT NULL, 
                                    PRIMARY KEY (ID), 
                                    CONSTRAINT FK_{TableNamePrefix}_NEO_BLOCK_TRANSACTION_INPUT FOREIGN KEY(TRANSACTION_ID) 
                                    REFERENCES {TableNamePrefix}_NEO_TRANSACTION(ID)
                                )'; 
                                EXECUTE IMMEDIATE 'CREATE TABLE {TableNamePrefix}_NEO_TRANSACTION_OUTPUT (
                                    ID varchar2(200) NOT NULL, 
                                    ADDRESS varchar2(200) NOT NULL,
                                    ASSET varchar2(200) NOT NULL,
                                    VALUE number(16,6) NOT NULL, 
                                    TRANSACTION_ID varchar2(200) NOT NULL, 
                                    PRIMARY KEY (ID), 
                                    CONSTRAINT FK_{TableNamePrefix}_NEO_BLOCK_TRANSACTION_OUTPUT FOREIGN KEY(TRANSACTION_ID) 
                                    REFERENCES {TableNamePrefix}_NEO_TRANSACTION(ID)
                                )'; 
                                END;";
                    default:
                        return String.Empty;
                }
            }
        }
        #endregion

        #region MSSQL
        public string CreateTableQueryMSSQL
        {
            get
            {
                return $@"IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{TableNamePrefix}{CryptoAdapterType.ToString()}Block'))
                            BEGIN 
                            {CreateTableBlockMSSQL}
                            END;
                          IF (NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = '{TableNamePrefix}{CryptoAdapterType.ToString()}Transaction'))
                            BEGIN 
                            {CreateTableTransactionMSSQL}
                            END;";
            }
        }

        private string CreateTableBlockMSSQL
        {
            get
            {
                switch (CryptoAdapterType)
                {
                    case CryptoAdapterType.Ethereum:
                    case CryptoAdapterType.Bitcoin:
                    case CryptoAdapterType.Litecoin:
                        return $@"CREATE TABLE {TableNamePrefix}{CryptoAdapterType.ToString()}Block (
                                Id nvarchar(200) PRIMARY KEY, 
                                BlockHeight bigint NOT NULL, 
                                TimeStamp datetime NOT NULL);";

                    case CryptoAdapterType.NEO:
                        return $@"CREATE TABLE {TableNamePrefix}NEOBlock (
                                Id nvarchar(200) PRIMARY KEY, 
                                BlockIndex bigint NOT NULL, 
                                TimeStamp datetime NOT NULL);";
                    default:
                        return String.Empty;
                }
            }
        }

        private string CreateTableTransactionMSSQL
        {
            get
            {
                switch (CryptoAdapterType)
                {
                    case CryptoAdapterType.Ethereum:
                        return $@"CREATE TABLE {TableNamePrefix}EthereumTransaction(
                                    Id nvarchar(200) PRIMARY KEY, 
                                    TxHash nvarchar(200) NOT NULL, 
                                    FromAccount nvarchar(500) NOT NULL, 
                                    ToAccount nvarchar(500) NULL, 
                                    Value decimal(16, 6) NOT NULL, 
                                    TxReceiptStatus nvarchar(20) NULL, 
                                    BlockId nvarchar(200) NOT NULL, 
                                    CONSTRAINT FK_{TableNamePrefix}EthereumBlockTransaction FOREIGN KEY (BlockId)
                                    REFERENCES {TableNamePrefix}EthereumBlock(Id)
                               );";

                    case CryptoAdapterType.Bitcoin:
                    case CryptoAdapterType.Litecoin:
                        return $@"CREATE TABLE {TableNamePrefix}{CryptoAdapterType.ToString()}Transaction(
                                    Id nvarchar(200) PRIMARY KEY, 
                                    TxHash nvarchar(200) NOT NULL, 
                                    TotalOutValue decimal(16, 6) NOT NULL, 
                                    BlockId nvarchar(200) NOT NULL, 
                                    CONSTRAINT FK_{TableNamePrefix}{CryptoAdapterType.ToString()}BlockTransaction FOREIGN KEY (BlockId)
                                    REFERENCES {TableNamePrefix}{CryptoAdapterType.ToString()}Block(Id)
                                );

                            CREATE TABLE {TableNamePrefix}{CryptoAdapterType.ToString()}TransactionInput(
                                Id nvarchar(200) PRIMARY KEY, 
                                Address nvarchar(500) NOT NULL, 
                                TransactionId nvarchar(200) NOT NULL, 
                                CONSTRAINT FK_{TableNamePrefix}{CryptoAdapterType.ToString()}TransactionInput FOREIGN KEY (TransactionId)
                                REFERENCES {TableNamePrefix}{CryptoAdapterType.ToString()}Transaction(Id)
                            );

                            CREATE TABLE {TableNamePrefix}{CryptoAdapterType.ToString()}TransactionOutput(
                                Id nvarchar(200) PRIMARY KEY, 
                                Address nvarchar(500) NOT NULL, 
                                Value decimal(16, 6) NOT NULL, 
                                TransactionId nvarchar(200) NOT NULL, 
                                CONSTRAINT FK_{TableNamePrefix}{CryptoAdapterType.ToString()}TransactionOutput FOREIGN KEY (TransactionId)
                                REFERENCES {TableNamePrefix}{CryptoAdapterType.ToString()}Transaction(Id)
                            );";

                    case CryptoAdapterType.NEO:
                        return $@"CREATE TABLE {TableNamePrefix}NEOTransaction(
                                    Id nvarchar(200) PRIMARY KEY, 
                                    TxId nvarchar(200) NOT NULL,
                                    TxType nvarchar(200) NOT NULL,
                                    BlockId nvarchar(200) NOT NULL, 
                                    CONSTRAINT FK_{TableNamePrefix}NEOBlockTransaction FOREIGN KEY (BlockId)
                                    REFERENCES {TableNamePrefix}NEOBlock(Id)
                                );

                            CREATE TABLE {TableNamePrefix}NEOTransactionInput(
                                Id nvarchar(200) PRIMARY KEY, 
                                TxInId nvarchar(200) NOT NULL, 
                                TransactionId nvarchar(200) NOT NULL, 
                                CONSTRAINT FK_{TableNamePrefix}NEOTransactionInput FOREIGN KEY (TransactionId)
                                REFERENCES {TableNamePrefix}NEOTransaction(Id)
                            );

                            CREATE TABLE {TableNamePrefix}NEOTransactionOutput(
                                Id nvarchar(200) PRIMARY KEY, 
                                Address nvarchar(500) NOT NULL, 
                                Asset nvarchar(500) NOT NULL,
                                Value decimal(16, 6) NOT NULL, 
                                TransactionId nvarchar(200) NOT NULL, 
                                CONSTRAINT FK_{TableNamePrefix}NEOTransactionOutput FOREIGN KEY (TransactionId)
                                REFERENCES {TableNamePrefix}NEOTransaction(Id)
                            );";
                    default:
                        return String.Empty;
                }
            }
        }

        #endregion
        #endregion

        #region Insert

        public Tuple<string, bool> InsertDataToDb(int insertQueryType, int pageNumber)
        {
            var queryInsert = new StringBuilder();
            var continuePaging = true;

            switch (CryptoAdapterType)
            {
                case CryptoAdapterType.Ethereum:

                    if (insertQueryType == (int)InsertQueryType.Blocks && EthereumBlockModel.Count > 0)
                    {
                        var blocksBatch = EthereumBlockModel.Skip(pageNumber * 1000).Take(1000);
                        if (blocksBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (blocksBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}EthereumBlock (Id, BlockHeight, TimeStamp) VALUES ");

                        foreach (var block in blocksBatch)
                        {
                            queryInsert.Append($"('{block.BlockIdSQL}', '{block.BlockNumber}', '{block.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.Transactions && EthereumBlockModel.SelectMany(e => e.BlockTransactions).Count() > 0)
                    {
                        var transactionsBatch = EthereumBlockModel.SelectMany(e => e.BlockTransactions).Skip(pageNumber * 1000).Take(1000);
                        if (transactionsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}EthereumTransaction(Id, TxHash, FromAccount, ToAccount, Value, TxReceiptStatus, BlockId) VALUES ");

                        foreach (var transaction in transactionsBatch)
                        {
                            queryInsert.Append($"('{transaction.TransactionIdSQL}', '{transaction.Hash}', '{transaction.From}', '{transaction.To}',{transaction.Value}, '{transaction.Status}' , '{transaction.ParentBlockId}'),");
                        }
                    }
                    else
                    {
                        return Tuple.Create(string.Empty, false);
                    }
                    break;

                case CryptoAdapterType.Bitcoin:

                    if (insertQueryType == (int)InsertQueryType.Blocks && BitcoinBlockModel.Count > 0)
                    {
                        var blocksBatch = BitcoinBlockModel.Skip(pageNumber * 1000).Take(1000);
                        if (blocksBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (blocksBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}BitcoinBlock (Id, BlockHeight, TimeStamp) VALUES ");

                        foreach (var block in blocksBatch)
                        {
                            queryInsert.Append($"('{block.BlockIdSQL}', '{block.BlockNumber}', '{block.Time.ToString("yyyy-MM-dd HH:mm:ss")}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.Transactions && BitcoinBlockModel.SelectMany(b => b.TransactionList).Count() > 0)
                    {
                        var transactionsBatch = BitcoinBlockModel.SelectMany(b => b.TransactionList).Skip(pageNumber * 1000).Take(1000);
                        if (transactionsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}BitcoinTransaction (Id, TxHash, TotalOutValue, BlockId) VALUES ");

                        foreach (var transaction in transactionsBatch)
                        {
                            queryInsert.Append($"('{transaction.TransactionIdSQL}', '{transaction.TransactionHash}', '{transaction.TotalOutValue}', '{transaction.ParentBlockIdSQL}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionInputs && BitcoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Count() > 0)
                    {
                        var transactionInputsBatch = BitcoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionInputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionInputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}BitcoinTransactionInput(Id, Address, TransactionId) VALUES ");

                        foreach (var transactionIn in transactionInputsBatch)
                        {
                            queryInsert.Append($"('{transactionIn.TransactionInIdSQL}', '{transactionIn.Address}', '{transactionIn.ParentTransactionIdSQL}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionOutputs && BitcoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Count() > 0)
                    {
                        var transactionOutputsBatch = BitcoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionOutputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionOutputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}BitcoinTransactionOutput(Id, Address, Value, TransactionId) VALUES ");

                        foreach (var transactionOut in transactionOutputsBatch)
                        {
                            queryInsert.Append($"('{transactionOut.TransactionOutIdSQL}', '{transactionOut.Address}', '{transactionOut.Value}', '{transactionOut.ParentTransactionIdSQL}'),");
                        }
                    }
                    else
                    {
                        return Tuple.Create(string.Empty, false);
                    }
                    break;

                case CryptoAdapterType.NEO:

                    if (insertQueryType == (int)InsertQueryType.Blocks && NEOBlockModel.Count > 0)
                    {
                        var blocksBatch = NEOBlockModel.Skip(pageNumber * 1000).Take(1000);
                        if (blocksBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (blocksBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}NEOBlock (Id, BlockIndex, TimeStamp) VALUES ");

                        foreach (var block in blocksBatch)
                        {
                            queryInsert.Append($"('{block.BlockIdSQL}', '{block.BlockNumber}', '{block.Time.ToString("yyyy-MM-dd HH:mm:ss")}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.Transactions && NEOBlockModel.SelectMany(b => b.TransactionList).Count() > 0)
                    {
                        var transactionsBatch = NEOBlockModel.SelectMany(b => b.TransactionList).Skip(pageNumber * 1000).Take(1000);
                        if (transactionsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}NEOTransaction (Id, TxId, TxType, BlockId) VALUES ");

                        foreach (var transaction in transactionsBatch)
                        {
                            queryInsert.Append($"('{transaction.TransactionIdSQL}', '{transaction.TransactionId}', '{transaction.TransactionType}', '{transaction.ParentBlockIdSQL}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionInputs && NEOBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Count() > 0)
                    {
                        var transactionInputsBatch = NEOBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionInputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionInputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}NEOTransactionInput(Id, TxInId, TransactionId) VALUES ");

                        foreach (var transactionIn in transactionInputsBatch)
                        {
                            queryInsert.Append($"('{transactionIn.TransactionInIdSQL}', '{transactionIn.TransactionId}', '{transactionIn.ParentTransactionIdSQL}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionOutputs && NEOBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Count() > 0)
                    {
                        var transactionOutputsBatch = NEOBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionOutputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionOutputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}NEOTransactionOutput(Id, Address, Asset, Value, TransactionId) VALUES ");

                        foreach (var transactionOut in transactionOutputsBatch)
                        {
                            queryInsert.Append($"('{transactionOut.TransactionOutIdSQL}', '{transactionOut.Address}', '{transactionOut.Asset}', '{transactionOut.Value}', '{transactionOut.ParentTransactionIdSQL}'),");
                        }
                    }
                    else
                    {
                        return Tuple.Create(string.Empty, false);
                    }
                    break;
                case CryptoAdapterType.Litecoin:

                    if (insertQueryType == (int)InsertQueryType.Blocks && LitecoinBlockModel.Count > 0)
                    {
                        var blocksBatch = LitecoinBlockModel.Skip(pageNumber * 1000).Take(1000);
                        if (blocksBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (blocksBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}LitecoinBlock (Id, BlockHeight, TimeStamp) VALUES ");

                        foreach (var block in blocksBatch)
                        {
                            queryInsert.Append($"('{block.BlockIdSQL}', '{block.BlockNumber}', '{block.Time.ToString("yyyy-MM-dd HH:mm:ss")}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.Transactions && LitecoinBlockModel.SelectMany(b => b.TransactionList).Count() > 0)
                    {
                        var transactionsBatch = LitecoinBlockModel.SelectMany(b => b.TransactionList).Skip(pageNumber * 1000).Take(1000);
                        if (transactionsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}LitecoinTransaction (Id, TxHash, TotalOutValue, BlockId) VALUES ");

                        foreach (var transaction in transactionsBatch)
                        {
                            queryInsert.Append($"('{transaction.TransactionIdSQL}', '{transaction.TransactionHash}', '{transaction.TotalOutValue}', '{transaction.ParentBlockIdSQL}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionInputs && LitecoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Count() > 0)
                    {
                        var transactionInputsBatch = LitecoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionInputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionInputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}LitecoinTransactionInput(Id, Address, TransactionId) VALUES ");

                        foreach (var transactionIn in transactionInputsBatch)
                        {
                            queryInsert.Append($"('{transactionIn.TransactionInIdSQL}', '{transactionIn.Address}', '{transactionIn.ParentTransactionIdSQL}'),");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionOutputs && LitecoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Count() > 0)
                    {
                        var transactionOutputsBatch = LitecoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionOutputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionOutputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}LitecoinTransactionOutput(Id, Address, Value, TransactionId) VALUES ");

                        foreach (var transactionOut in transactionOutputsBatch)
                        {
                            queryInsert.Append($"('{transactionOut.TransactionOutIdSQL}', '{transactionOut.Address}', '{transactionOut.Value}', '{transactionOut.ParentTransactionIdSQL}'),");
                        }
                    }
                    else
                    {
                        return Tuple.Create(string.Empty, false);
                    }
                    break;
            }

            queryInsert.Length--;
            queryInsert.AppendLine("; ");

            return Tuple.Create(queryInsert.ToString(), continuePaging);
        }


        public Tuple<string, bool> InsertDataToOracleDb(int insertQueryType, int pageNumber)
        {
            var queryInsert = new StringBuilder();
            queryInsert.AppendLine("BEGIN");
            var continuePaging = true;

            switch (CryptoAdapterType)
            {
                case CryptoAdapterType.Ethereum:

                    if (insertQueryType == (int)InsertQueryType.Blocks && EthereumBlockModel.Count > 0)
                    {
                        var blocksBatch = EthereumBlockModel.Skip(pageNumber * 1000).Take(1000);
                        if (blocksBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (blocksBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_ETHEREUM_BLOCK (ID, BLOCK_HEIGHT, TIMESTAMP)");

                        foreach (var block in blocksBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{block.BlockIdSQL}', '{block.BlockNumber}', TO_TIMESTAMP('{block.TimeStamp.ToString("yyyy-MM-dd HH:mm:ss")}', 'YYYY-MM-DD:HH24:MI:SS') FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.Transactions && EthereumBlockModel.SelectMany(e => e.BlockTransactions).Count() > 0)
                    {
                        var transactionsBatch = EthereumBlockModel.SelectMany(e => e.BlockTransactions).Skip(pageNumber * 1000).Take(1000);
                        if (transactionsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_ETHEREUM_TRANSACTION (ID, TX_HASH, FROM_ACCOUNT, TO_ACCOUNT, VALUE, TX_RECEIPT_STATUS, BLOCK_ID)");

                        foreach (var transaction in transactionsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transaction.TransactionIdSQL}','{transaction.Hash}','{transaction.From}','{transaction.To}','{transaction.Value}','{transaction.Status}', '{transaction.ParentBlockId}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else
                    {
                        return Tuple.Create(string.Empty, false);
                    }
                    break;

                case CryptoAdapterType.Bitcoin:

                    if (insertQueryType == (int)InsertQueryType.Blocks && BitcoinBlockModel.Count > 0)
                    {
                        var blocksBatch = BitcoinBlockModel.Skip(pageNumber * 1000).Take(1000);
                        if (blocksBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (blocksBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_BITCOIN_BLOCK (ID, BLOCK_HEIGHT, TIMESTAMP)");

                        foreach (var block in blocksBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{block.BlockIdSQL}', '{block.BlockNumber}', TO_TIMESTAMP('{block.Time.ToString("yyyy-MM-dd HH:mm:ss")}', 'YYYY-MM-DD:HH24:MI:SS') FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.Transactions && BitcoinBlockModel.SelectMany(b => b.TransactionList).Count() > 0)
                    {
                        var transactionsBatch = BitcoinBlockModel.SelectMany(b => b.TransactionList).Skip(pageNumber * 1000).Take(1000);
                        if (transactionsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_BITCOIN_TRANSACTION (ID, TX_HASH, TOTAL_OUT_VALUE, BLOCK_ID)");

                        foreach (var transaction in transactionsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transaction.TransactionIdSQL}','{transaction.TransactionHash}','{transaction.TotalOutValue}', '{transaction.ParentBlockIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionInputs && BitcoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Count() > 0)
                    {
                        var transactionInputsBatch = BitcoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionInputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionInputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_BITCOIN_TRANSACTION_INPUT (ID, ADDRESS, TRANSACTION_ID)");

                        foreach (var transactionIn in transactionInputsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transactionIn.TransactionInIdSQL}','{transactionIn.Address}', '{transactionIn.ParentTransactionIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }

                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionOutputs && BitcoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Count() > 0)
                    {
                        var transactionOutputsBatch = BitcoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionOutputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionOutputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_BITCOIN_TRANSACTION_OUTPUT (ID, ADDRESS, VALUE, TRANSACTION_ID)");

                        foreach (var transactionOut in transactionOutputsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transactionOut.TransactionOutIdSQL}','{transactionOut.Address}','{transactionOut.Value}', '{transactionOut.ParentTransactionIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else
                    {
                        return Tuple.Create(string.Empty, false);
                    }
                    break;

                case CryptoAdapterType.NEO:

                    if (insertQueryType == (int)InsertQueryType.Blocks && NEOBlockModel.Count > 0)
                    {
                        var blocksBatch = NEOBlockModel.Skip(pageNumber * 1000).Take(1000);
                        if (blocksBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (blocksBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_NEO_BLOCK (ID, BLOCK_INDEX, TIMESTAMP)");

                        foreach (var block in blocksBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{block.BlockIdSQL}', '{block.BlockNumber}', TO_TIMESTAMP('{block.Time.ToString("yyyy-MM-dd HH:mm:ss")}', 'YYYY-MM-DD:HH24:MI:SS') FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.Transactions && NEOBlockModel.SelectMany(b => b.TransactionList).Count() > 0)
                    {
                        var transactionsBatch = NEOBlockModel.SelectMany(b => b.TransactionList).Skip(pageNumber * 1000).Take(1000);
                        if (transactionsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_NEO_TRANSACTION (ID, TX_ID, TX_TYPE, BLOCK_ID)");

                        foreach (var transaction in transactionsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transaction.TransactionIdSQL}','{transaction.TransactionId}','{transaction.TransactionType}', '{transaction.ParentBlockIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionInputs && NEOBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Count() > 0)
                    {
                        var transactionInputsBatch = NEOBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionInputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionInputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_NEO_TRANSACTION_INPUT (ID, TX_IN_ID, TRANSACTION_ID)");

                        foreach (var transactionIn in transactionInputsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transactionIn.TransactionInIdSQL}','{transactionIn.TransactionId}', '{transactionIn.ParentTransactionIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionOutputs && NEOBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Count() > 0)
                    {
                        var transactionOutputsBatch = NEOBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionOutputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionOutputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_NEO_TRANSACTION_OUTPUT (ID, ADDRESS, ASSET, VALUE, TRANSACTION_ID)");

                        foreach (var transactionOut in transactionOutputsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transactionOut.TransactionOutIdSQL}','{transactionOut.Address}','{transactionOut.Asset}','{transactionOut.Value}', '{transactionOut.ParentTransactionIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else
                    {
                        return Tuple.Create(string.Empty, false);
                    }
                    break;
                case CryptoAdapterType.Litecoin:

                    if (insertQueryType == (int)InsertQueryType.Blocks && LitecoinBlockModel.Count > 0)
                    {
                        var blocksBatch = LitecoinBlockModel.Skip(pageNumber * 1000).Take(1000);
                        if (blocksBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (blocksBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_LITECOIN_BLOCK (ID, BLOCK_HEIGHT, TIMESTAMP)");

                        foreach (var block in blocksBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{block.BlockIdSQL}', '{block.BlockNumber}', TO_TIMESTAMP('{block.Time.ToString("yyyy-MM-dd HH:mm:ss")}', 'YYYY-MM-DD:HH24:MI:SS') FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.Transactions && LitecoinBlockModel.SelectMany(b => b.TransactionList).Count() > 0)
                    {
                        var transactionsBatch = LitecoinBlockModel.SelectMany(b => b.TransactionList).Skip(pageNumber * 1000).Take(1000);
                        if (transactionsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_LITECOIN_TRANSACTION (ID, TX_HASH, TOTAL_OUT_VALUE, BLOCK_ID)");

                        foreach (var transaction in transactionsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transaction.TransactionIdSQL}','{transaction.TransactionHash}','{transaction.TotalOutValue}', '{transaction.ParentBlockIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionInputs && LitecoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Count() > 0)
                    {
                        var transactionInputsBatch = LitecoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionInputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionInputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionInputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_LITECOIN_TRANSACTION_INPUT (ID, ADDRESS, TRANSACTION_ID)");

                        foreach (var transactionIn in transactionInputsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transactionIn.TransactionInIdSQL}','{transactionIn.Address}', '{transactionIn.ParentTransactionIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }

                    }
                    else if (insertQueryType == (int)InsertQueryType.TransactionOutputs && LitecoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Count() > 0)
                    {
                        var transactionOutputsBatch = LitecoinBlockModel.SelectMany(b => b.TransactionList).SelectMany(t => t.TransactionOutputs).Skip(pageNumber * 1000).Take(1000);
                        if (transactionOutputsBatch.Count() < 1000)
                        {
                            continuePaging = false;
                            if (transactionOutputsBatch.Count() == 0)
                            {
                                return Tuple.Create(String.Empty, continuePaging);
                            }
                        }

                        queryInsert.AppendLine($"INSERT INTO {TableNamePrefix}_LITECOIN_TRANSACTION_OUTPUT (ID, ADDRESS, VALUE, TRANSACTION_ID)");

                        foreach (var transactionOut in transactionOutputsBatch)
                        {
                            queryInsert.AppendLine($"SELECT '{transactionOut.TransactionOutIdSQL}','{transactionOut.Address}','{transactionOut.Value}', '{transactionOut.ParentTransactionIdSQL}' FROM dual");
                            queryInsert.Append("UNION ALL ");
                        }
                    }
                    else
                    {
                        return Tuple.Create(string.Empty, false);
                    }
                    break;
            }

            queryInsert.Remove((queryInsert.Length - 10), 10);
            queryInsert.AppendLine(";");
            queryInsert.AppendLine("END;");

            return Tuple.Create(queryInsert.ToString(), continuePaging);
        }
        #endregion
    }
}
