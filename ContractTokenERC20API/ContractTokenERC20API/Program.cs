using System;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using Nethereum.RPC.Eth.DTOs;
using System.Numerics;
using Nethereum.Hex.HexTypes;
using System.IO;
using Nethereum.Signer;
using System.Reflection;
using Nethereum.JsonRpc.WebSocketStreamingClient;
using Nethereum.RPC.Reactive.Eth.Subscriptions;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Nethereum.RPC.Reactive.Eth;
using System.Threading;
using System.Linq;
using System.Collections.Generic;

namespace ContractTokenERC20API
{
    class Program
    {
        const int UNLOCK_TIMEOUT = 2 * 60; // 2 minutes (arbitrary)
        const int SLEEP_TIME = 5 * 1000; // 5 seconds (arbitrary)
        const int MAX_TIMEOUT = 2 * 60 * 1000; // 2 minutes (arbirtrary)

        // These static public variables do not represent a recommended pattern
        static public string LastProtocolVersion = "";
        static public string LastTxHash = "";
        static public Nethereum.RPC.Eth.DTOs.TransactionReceipt LastTxReceipt = null;
        static public HexBigInteger LastMaxBlockNumber = new HexBigInteger(0);

        public static string privateKey = "0xda3576262901821e2a6ec56230a6280cfd29ceed794179cc85371db6aa099910";
        public static string ownerAddress = "0x29D03c460A726F8e07A140887AD0a940bA9Ef092";// ETH Address
        public static string contractAddress = "0x040eda53da3c1025ce5e878f8d39702f02c5275f";
        public static string newAddress = "0xFd99BddB26e99448d1ce47A3b7976BD247eC8a77";
        public static string abi = File.ReadAllText(@"D:\Private\MyProjects\ContractTokenERC20API\ContractTokenERC20API\ContractTokenERC20API\abi.json");
        //var contractByteCode = "0x608060405234801561001057600080fd5b50600436106100ea5760003560e01c806395d89b411161008c578063b5931f7c11610066578063b5931f7c1461044b578063d05c78da14610497578063dd62ed3e146104e3578063e6cb90131461055b576100ea565b806395d89b4114610316578063a293d1e814610399578063a9059cbb146103e5576100ea565b806323b872dd116100c857806323b872dd146101f6578063313ce5671461027c5780633eaaf86b146102a057806370a08231146102be576100ea565b806306fdde03146100ef578063095ea7b31461017257806318160ddd146101d8575b600080fd5b6100f76105a7565b6040518080602001828103825283818151815260200191508051906020019080838360005b8381101561013757808201518184015260208101905061011c565b50505050905090810190601f1680156101645780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6101be6004803603604081101561018857600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919080359060200190929190505050610645565b604051808215151515815260200191505060405180910390f35b6101e0610737565b6040518082815260200191505060405180910390f35b6102626004803603606081101561020c57600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff16906020019092919080359060200190929190505050610782565b604051808215151515815260200191505060405180910390f35b610284610a12565b604051808260ff1660ff16815260200191505060405180910390f35b6102a8610a25565b6040518082815260200191505060405180910390f35b610300600480360360208110156102d457600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610a2b565b6040518082815260200191505060405180910390f35b61031e610a74565b6040518080602001828103825283818151815260200191508051906020019080838360005b8381101561035e578082015181840152602081019050610343565b50505050905090810190601f16801561038b5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6103cf600480360360408110156103af57600080fd5b810190808035906020019092919080359060200190929190505050610b12565b6040518082815260200191505060405180910390f35b610431600480360360408110156103fb57600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919080359060200190929190505050610b2c565b604051808215151515815260200191505060405180910390f35b6104816004803603604081101561046157600080fd5b810190808035906020019092919080359060200190929190505050610cb5565b6040518082815260200191505060405180910390f35b6104cd600480360360408110156104ad57600080fd5b810190808035906020019092919080359060200190929190505050610cd5565b6040518082815260200191505060405180910390f35b610545600480360360408110156104f957600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610d02565b6040518082815260200191505060405180910390f35b6105916004803603604081101561057157600080fd5b810190808035906020019092919080359060200190929190505050610d89565b6040518082815260200191505060405180910390f35b60008054600181600116156101000203166002900480601f01602080910402602001604051908101604052809291908181526020018280546001816001161561010002031660029004801561063d5780601f106106125761010080835404028352916020019161063d565b820191906000526020600020905b81548152906001019060200180831161062057829003601f168201915b505050505081565b600081600560003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167f8c5be1e5ebec7d5bd14f71427d1e84f3dd0314c0f7b2291e5b200ac8c7c3b925846040518082815260200191505060405180910390a36001905092915050565b6000600460008073ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205460035403905090565b60006107cd600460008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610b12565b600460008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002081905550610896600560008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610b12565b600560008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000208190555061095f600460008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610d89565b600460008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508273ffffffffffffffffffffffffffffffffffffffff168473ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040518082815260200191505060405180910390a3600190509392505050565b600260009054906101000a900460ff1681565b60035481565b6000600460008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020549050919050565b60018054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610b0a5780601f10610adf57610100808354040283529160200191610b0a565b820191906000526020600020905b815481529060010190602001808311610aed57829003601f168201915b505050505081565b600082821115610b2157600080fd5b818303905092915050565b6000610b77600460003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610b12565b600460003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002081905550610c03600460008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610d89565b600460008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040518082815260200191505060405180910390a36001905092915050565b6000808211610cc357600080fd5b818381610ccc57fe5b04905092915050565b600081830290506000831480610cf3575081838281610cf057fe5b04145b610cfc57600080fd5b92915050565b6000600560008473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002054905092915050565b6000818301905082811015610d9d57600080fd5b9291505056fea265627a7a723158207195fb41568494530a5843cffe6c58576a55c32ab2a17e9c715d83d76c331d1564736f6c63430005110032";
        public static Account account = new Account(privateKey, Chain.Ropsten);

        public static Web3 web3 = new Web3(account, "https://ropsten.infura.io/v3/4ca92ff296f346f7989c4b056a93ff3a");

        public static List<TransactionNeedConfirm> TransactionNeedConfirms = new List<TransactionNeedConfirm>();

        static async System.Threading.Tasks.Task Main(string[] args)
        {
            var balance = await web3.Eth.GetBalance.SendRequestAsync(ownerAddress);
            Console.WriteLine($"Balance in Wei: {balance.Value}");
            var etherAmount = Web3.Convert.FromWei(balance.Value);
            Console.WriteLine($"Balance in Ether: {etherAmount}");

            #region Send Token
            //var handler = web3.Eth.GetContractHandler(contractAddress);
            //var balanceMessage = new BalanceOfFunction() { Owner = ownerAddress };
            //var tokenBalance = await handler.QueryAsync<BalanceOfFunction, BigInteger>(balanceMessage);
            //var a = Web3.Convert.FromWei(tokenBalance); ;
            //Console.WriteLine($"Balance in Token: {a}");

            //var contract = web3.Eth.GetContract(abi, contractAddress);
            //var transferFn = contract.GetFunction("transfer");
            //var amountToken = 10000000000000000000M;
            //var gas = await transferFn.EstimateGasAsync(ownerAddress, null, null, newAddress, amountToken);
            //var value = new HexBigInteger("200000000000");
            //var transactionHash = await transferFn.SendTransactionAsync(ownerAddress, gas, null, null, newAddress, amountToken);
            //Console.WriteLine($"Hash: {transactionHash}");
            #endregion

            #region Subcribe and Check Receive ETH
            using (var client = new StreamingWebSocketClient("wss://ropsten.infura.io/ws/v3/4ca92ff296f346f7989c4b056a93ff3a"))
            {
                //create the subscription
                //(it won't start receiving data until Subscribe is called)

                var subscriptionNewBlock = new EthNewBlockHeadersObservableSubscription(client);

                var pendingTransactionsSubscription = new EthNewPendingTransactionObservableSubscription(client);

                var logsSubscription = new EthLogsObservableSubscription(client);

                //attach a handler for when the subscription is first created (optional)
                //this will occur once after Subscribe has been called
                subscriptionNewBlock.GetSubscribeResponseAsObservable().Subscribe(id =>
                   Console.WriteLine("Block Header subscription Id: " + id));

                pendingTransactionsSubscription.GetSubscribeResponseAsObservable().Subscribe(id =>
                    Console.WriteLine("Transactions SubscriptionId:" + id));

                logsSubscription.GetSubscribeResponseAsObservable().Subscribe(id =>
                    Console.WriteLine("Logs SubscriptionId:" + id));

                DateTime? lastBlockNotification = null;

                //attach a handler for each block
                //put your logic here
                subscriptionNewBlock.GetSubscriptionDataResponsesAsObservable().Subscribe(block =>
                {
                    lastBlockNotification = DateTime.Now;
                    var utcTimestamp = DateTimeOffset.FromUnixTimeSeconds((long)block.Timestamp.Value);
                    Console.WriteLine($"New Block Number: {block.Number.Value}");
                    //Console.WriteLine($"Block Hash: {block.BlockHash}");
                    Console.WriteLine($"-------------------------------------------------------------");

                    if (TransactionNeedConfirms.Any())
                    {
                        foreach(var item in TransactionNeedConfirms)
                        {
                            item.Confirmed = (int)(block.Number.Value - item.NumberBlock);
                            Console.WriteLine($"***************************************************");
                            Console.WriteLine($"*******************YOUR TRANSACTION HASH*******************");
                            Console.WriteLine($"Transaction Hash: {item.Hash}");
                            Console.WriteLine($"COMFIRMED: {item.Confirmed}");
                            Console.WriteLine($"FROM: {item.FromAddress}");
                            Console.WriteLine($"TO: {item.ToAddress}");
                            Console.WriteLine($"VALUE: {item.Value}");

                            Console.WriteLine($"***************************************************");
                        }
                    }
                    
                    var blockTransaction = web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(block.Number).Result;
                    if(blockTransaction != null)
                    {
                        var transactionTransferEthList = blockTransaction.Transactions.Where(m => m.Input == "0x").ToList();
                        foreach(var transfer in transactionTransferEthList)
                        {
                            if(string.Equals(transfer.To, ownerAddress, StringComparison.InvariantCultureIgnoreCase))
                            {
                                TransactionNeedConfirms.Add(new TransactionNeedConfirm
                                {
                                    Hash = transfer.TransactionHash,
                                    FromAddress = transfer.From,
                                    ToAddress = transfer.To,
                                    NumberBlock = block.Number.Value,
                                    Confirmed = 1,
                                    Value = transfer.Value,
                                });

                                Console.WriteLine($"------*******------TO Your Address--------*******-----------");
                                Console.WriteLine($"Transaction hash: {transfer.TransactionHash}");
                                Console.WriteLine($"Value: {transfer.Value}");
                                Console.WriteLine($"From: {transfer.From}");
                                Console.WriteLine($"To: {transfer.To}");
                                Console.WriteLine($"---------------***********************-----------------------");
                            }
                            else if (string.Equals(transfer.From, ownerAddress, StringComparison.InvariantCultureIgnoreCase))
                            {
                                TransactionNeedConfirms.Add(new TransactionNeedConfirm
                                {
                                    Hash = transfer.TransactionHash,
                                    FromAddress = transfer.From,
                                    ToAddress = transfer.To,
                                    NumberBlock = block.Number.Value,
                                    Confirmed = 1,
                                    Value = transfer.Value,
                                });

                                Console.WriteLine($"------*******------FROM Your Address--------*******-----------");
                                Console.WriteLine($"Transaction hash: {transfer.TransactionHash}");
                                Console.WriteLine($"Value: {transfer.Value}");
                                Console.WriteLine($"From: {transfer.From}");
                                Console.WriteLine($"To: {transfer.To}");
                                Console.WriteLine($"---------------***********************-----------------------");
                            }
                            //else
                            //{
                            //    Console.WriteLine($"---------------------Other Address---------------------------");
                            //    Console.WriteLine($"Transaction hash: {transfer.TransactionHash}");
                            //    Console.WriteLine($"Value: {transfer.Value}");
                            //    Console.WriteLine($"From: {transfer.From}");
                            //    Console.WriteLine($"To: {transfer.To}");
                            //    Console.WriteLine($"-------------------------------------------------------------");
                            //}
                        }
                    }
                });

                pendingTransactionsSubscription.GetSubscriptionDataResponsesAsObservable().Subscribe(transactionHash =>
                {
                    //Console.WriteLine("Pending Transaction Hash: " + transactionHash);
                    //Console.WriteLine($"--------End Transaction Hash----------------");
                });

                logsSubscription.GetSubscriptionDataResponsesAsObservable().Subscribe(log =>
                {
                    //Console.WriteLine($"----------Start Block Hash--------------");
                    //Console.WriteLine("Block Hash: " + log.BlockHash);
                    //Console.WriteLine("Transaction Hash: " + log.TransactionHash + " of address: " + log.Address);
                   /// Console.WriteLine($"----------End Block Hash--------------");

                });


                bool subscribed = true;

                //handle unsubscription
                //optional - but may be important depending on your use case
                subscriptionNewBlock.GetUnsubscribeResponseAsObservable().Subscribe(response =>
                {
                    subscribed = false;
                    Console.WriteLine("Block Header unsubscribe result: " + response);
                });

                pendingTransactionsSubscription.GetUnsubscribeResponseAsObservable().Subscribe(response =>
                {
                    subscribed = false;
                    Console.WriteLine("Pending Transaction unsubscribe result: " + response);
                });

                logsSubscription.GetUnsubscribeResponseAsObservable().Subscribe(response =>
                {
                    subscribed = false;
                    Console.WriteLine("Logs unsubscribe result: " + response);
                });

                //open the websocket connection
                await client.StartAsync();

                //start the subscription
                // this will only block long enough to register the subscription with the client
                // once running - it won't block whilst waiting for blocks
                // blocks will be delivered to our handler on another thread
                await subscriptionNewBlock.SubscribeAsync();
                await pendingTransactionsSubscription.SubscribeAsync();
                //await logsSubscription.SubscribeAsync();

                //run for a minute before unsubscribing

                await Task.Delay(TimeSpan.FromMinutes(10));

                //unsubscribe
                await subscriptionNewBlock.UnsubscribeAsync();
                await pendingTransactionsSubscription.UnsubscribeAsync();
                //await logsSubscription.UnsubscribeAsync();
                //allow time to unsubscribe
                while (subscribed) await Task.Delay(TimeSpan.FromSeconds(1));
            }
            #endregion

            #region subcribe Pair price
            //string uniSwapFactoryAddress = "0x5C69bEe701ef814a2B6a3EDD4B1652CB9cc5aA6f";

            //var _web3 = new Web3("https://mainnet.infura.io/v3/4ca92ff296f346f7989c4b056a93ff3a");

            //string daiAddress = "0x6b175474e89094c44da98b954eedeac495271d0f";
            //string wethAddress = "0xc02aaa39b223fe8d0a0e5c4f27ead9083c756cc2";

            //var pairContractAddress = await web3.Eth.GetContractQueryHandler<GetPairFunction>()
            //    .QueryAsync<string>(uniSwapFactoryAddress,
            //        new GetPairFunction() { TokenA = daiAddress, TokenB = wethAddress });

            //var filter = web3.Eth.GetEvent<PairSyncEventDTO>(pairContractAddress).CreateFilterInput();
            //using (var client = new StreamingWebSocketClient("wss://mainnet.infura.io/ws/v3/7238211010344719ad14a89db874158c"))
            //{
            //    var subscription = new EthLogsObservableSubscription(client);
            //    subscription.GetSubscriptionDataResponsesAsObservable().
            //                 Subscribe(log =>
            //                 {
            //                     try
            //                     {
            //                         EventLog<PairSyncEventDTO> decoded = Event<PairSyncEventDTO>.DecodeEvent(log);
            //                         if (decoded != null)
            //                         {
            //                             decimal reserve0 = Web3.Convert.FromWei(decoded.Event.Reserve0);
            //                             decimal reserve1 = Web3.Convert.FromWei(decoded.Event.Reserve1);
            //                             Console.WriteLine($@"Price={reserve0 / reserve1}");
            //                         }
            //                         else Console.WriteLine(@"Found not standard transfer log");
            //                     }
            //                     catch (Exception ex)
            //                     {
            //                         Console.WriteLine(@"Log Address: " + log.Address + @" is not a standard transfer log:", ex.Message);
            //                     }
            //                 });

            //    bool subscribed = true;

            //    subscription.GetUnsubscribeResponseAsObservable().Subscribe(response =>
            //        {
            //            subscribed = false;
            //            Console.WriteLine("Block Header unsubscribe result: " + response);
            //        });

            //    await client.StartAsync();
            //    subscription.GetSubscribeResponseAsObservable().Subscribe(id => Console.WriteLine($"Subscribed with id: {id}"));
            //    await subscription.SubscribeAsync(filter);



            //    await Task.Delay(TimeSpan.FromMinutes(1));

            //    await subscription.UnsubscribeAsync();

            //    while (subscribed) await Task.Delay(TimeSpan.FromSeconds(1));
            //}
            #endregion
            Console.ReadLine();
        }

        public static async Task<Nethereum.RPC.Eth.DTOs.Transaction> CheckHash(string hash)
        {
            var txHash = "0x9eee045f8fab93a6f056cd5eb13b1986f0a88f626001b8d91dcfee0d25c27f2c";
            int timeoutCount = 0;
            var txReceipt = await web3.Eth.Transactions.GetTransactionByHash.SendRequestAsync(hash);
            //var a = await web3.Eth.Transactions.GetTransactionByBlockNumberAndIndex.SendRequestAsync(txReceipt.BlockNumber, txReceipt.TransactionIndex);
            //var c = await web3.Eth.Blocks.GetBlockWithTransactionsByNumber.SendRequestAsync(txReceipt.BlockNumber);
            var latestBlockNumber = await web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

            

            //var blockTransaction = web3.Eth.Blocks.GetBlockTransactionCountByHash;
            //var b = await blockTransaction.SendRequestAsync(txHash);

            while (txReceipt == null && timeoutCount < MAX_TIMEOUT)
            {
                Console.WriteLine("Sleeping...");
                Thread.Sleep(SLEEP_TIME);
                //txReceipt = await web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(hash);
                timeoutCount += SLEEP_TIME;
            }
            var confirmed = latestBlockNumber.Value - txReceipt.BlockNumber.Value;
            Console.WriteLine("timeoutCount " + timeoutCount.ToString());
            Console.WriteLine("confirmed " + confirmed);

            //LastTxReceipt = txReceipt;

            return txReceipt;
        }

        public class StandardTokenDeployment : ContractDeploymentMessage
        {

            public static string BYTECODE = "0x608060405234801561001057600080fd5b50600436106100ea5760003560e01c806395d89b411161008c578063b5931f7c11610066578063b5931f7c1461044b578063d05c78da14610497578063dd62ed3e146104e3578063e6cb90131461055b576100ea565b806395d89b4114610316578063a293d1e814610399578063a9059cbb146103e5576100ea565b806323b872dd116100c857806323b872dd146101f6578063313ce5671461027c5780633eaaf86b146102a057806370a08231146102be576100ea565b806306fdde03146100ef578063095ea7b31461017257806318160ddd146101d8575b600080fd5b6100f76105a7565b6040518080602001828103825283818151815260200191508051906020019080838360005b8381101561013757808201518184015260208101905061011c565b50505050905090810190601f1680156101645780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6101be6004803603604081101561018857600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919080359060200190929190505050610645565b604051808215151515815260200191505060405180910390f35b6101e0610737565b6040518082815260200191505060405180910390f35b6102626004803603606081101561020c57600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff16906020019092919080359060200190929190505050610782565b604051808215151515815260200191505060405180910390f35b610284610a12565b604051808260ff1660ff16815260200191505060405180910390f35b6102a8610a25565b6040518082815260200191505060405180910390f35b610300600480360360208110156102d457600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610a2b565b6040518082815260200191505060405180910390f35b61031e610a74565b6040518080602001828103825283818151815260200191508051906020019080838360005b8381101561035e578082015181840152602081019050610343565b50505050905090810190601f16801561038b5780820380516001836020036101000a031916815260200191505b509250505060405180910390f35b6103cf600480360360408110156103af57600080fd5b810190808035906020019092919080359060200190929190505050610b12565b6040518082815260200191505060405180910390f35b610431600480360360408110156103fb57600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff16906020019092919080359060200190929190505050610b2c565b604051808215151515815260200191505060405180910390f35b6104816004803603604081101561046157600080fd5b810190808035906020019092919080359060200190929190505050610cb5565b6040518082815260200191505060405180910390f35b6104cd600480360360408110156104ad57600080fd5b810190808035906020019092919080359060200190929190505050610cd5565b6040518082815260200191505060405180910390f35b610545600480360360408110156104f957600080fd5b81019080803573ffffffffffffffffffffffffffffffffffffffff169060200190929190803573ffffffffffffffffffffffffffffffffffffffff169060200190929190505050610d02565b6040518082815260200191505060405180910390f35b6105916004803603604081101561057157600080fd5b810190808035906020019092919080359060200190929190505050610d89565b6040518082815260200191505060405180910390f35b60008054600181600116156101000203166002900480601f01602080910402602001604051908101604052809291908181526020018280546001816001161561010002031660029004801561063d5780601f106106125761010080835404028352916020019161063d565b820191906000526020600020905b81548152906001019060200180831161062057829003601f168201915b505050505081565b600081600560003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167f8c5be1e5ebec7d5bd14f71427d1e84f3dd0314c0f7b2291e5b200ac8c7c3b925846040518082815260200191505060405180910390a36001905092915050565b6000600460008073ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205460035403905090565b60006107cd600460008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610b12565b600460008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002081905550610896600560008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610b12565b600560008673ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000208190555061095f600460008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610d89565b600460008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508273ffffffffffffffffffffffffffffffffffffffff168473ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040518082815260200191505060405180910390a3600190509392505050565b600260009054906101000a900460ff1681565b60035481565b6000600460008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020549050919050565b60018054600181600116156101000203166002900480601f016020809104026020016040519081016040528092919081815260200182805460018160011615610100020316600290048015610b0a5780601f10610adf57610100808354040283529160200191610b0a565b820191906000526020600020905b815481529060010190602001808311610aed57829003601f168201915b505050505081565b600082821115610b2157600080fd5b818303905092915050565b6000610b77600460003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610b12565b600460003373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002081905550610c03600460008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff1681526020019081526020016000205483610d89565b600460008573ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff168152602001908152602001600020819055508273ffffffffffffffffffffffffffffffffffffffff163373ffffffffffffffffffffffffffffffffffffffff167fddf252ad1be2c89b69c2b068fc378daa952ba7f163c4a11628f55a4df523b3ef846040518082815260200191505060405180910390a36001905092915050565b6000808211610cc357600080fd5b818381610ccc57fe5b04905092915050565b600081830290506000831480610cf3575081838281610cf057fe5b04145b610cfc57600080fd5b92915050565b6000600560008473ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002060008373ffffffffffffffffffffffffffffffffffffffff1673ffffffffffffffffffffffffffffffffffffffff16815260200190815260200160002054905092915050565b6000818301905082811015610d9d57600080fd5b9291505056fea265627a7a723158207195fb41568494530a5843cffe6c58576a55c32ab2a17e9c715d83d76c331d1564736f6c63430005110032";

            public StandardTokenDeployment() : base(BYTECODE) { }

            [Parameter("uint256", "totalSupply")]
            public BigInteger TotalSupply { get; set; }
        }


        [Function("transfer", "bool")]
        public class TransferFunction : FunctionMessage
        {
            [Parameter("address", "_to", 1)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 2)]
            public int TokenAmount { get; set; }
        }

        [Function("balanceOf", "uint256")]
        public class BalanceOfFunction : FunctionMessage
        {
            [Parameter("address", "_owner", 1)]
            public string Owner { get; set; }
        }

        [Event("Transfer")]
        public class TransferEventDTO : IEventDTO
        {
            [Parameter("address", "_from", 1, true)]
            public string From { get; set; }

            [Parameter("address", "_to", 2, true)]
            public string To { get; set; }

            [Parameter("uint256", "_value", 3, false)]
            public BigInteger Value { get; set; }
        }

        [Event("Sync")]
        class PairSyncEventDTO : IEventDTO
        {
            [Parameter("uint112", "reserve0")]
            public virtual BigInteger Reserve0 { get; set; }

            [Parameter("uint112", "reserve1", 2)]
            public virtual BigInteger Reserve1 { get; set; }
        }


        public partial class GetPairFunction : GetPairFunctionBase { }

        [Function("getPair", "address")]
        public class GetPairFunctionBase : FunctionMessage
        {
            [Parameter("address", "tokenA", 1)]
            public virtual string TokenA { get; set; }
            [Parameter("address", "tokenB", 2)]
            public virtual string TokenB { get; set; }
        }

        public class TransactionNeedConfirm
        {
            public string Hash { get; set; }
            public string FromAddress { get; set; }
            public string ToAddress { get; set; }
            public BigInteger NumberBlock { get; set; }
            public int Confirmed { get; set; }
            public HexBigInteger Value { get; set; }
        }
    }
}
