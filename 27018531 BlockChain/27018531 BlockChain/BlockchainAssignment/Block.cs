using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Block
    {
        // Boolean for threading
        public Boolean THREADING { get; set; } = true;
        // List of transactions 
        public List<Transaction> transactionList { get; set; }
        // Time 
        public DateTime timeStamp { get; set; }
        // Position of block 
        public int index { get; set; }
        // difficulty
        public int difficulty { get; set; }
        // Number used once for Proof-of-Work and mining
        public long nonce { get; set; } = 0;
        // current blocks "identity"
        public string hash { get; set; }
        // pointer to previous block
        public string prevHash { get; set; }
        // merkle root of all transactions block
        public string merkleRoot { get; set; } = "0";
        // Public Key wallet address of miner
        public string minerAddress { get; set; }
        // Reward
        public double reward { get; set; } = 1;                                     

        // private int nonce = 0;
        public int nonce0 { get; set; } = 0;
        // private int nonce = 1;
        public int nonce1 { get; set; } = 1;
     
        public string finalHash { get; set; }
        public string finalHash0 { get; set; }
        public string finalHash1 { get; set; }
        private bool th1Fin = false, th2Fin = false;

        // Constructor for previous block
        public Block(Block lastBlock) {
            this.timeStamp = DateTime.Now;
            this.nonce = 0;
            this.index = lastBlock.index + 1;
            this.prevHash = lastBlock.hash;
            if (THREADING == true){
                ThreadedMine();
                this.hash = this.finalHash;
            }
            else this.hash = this.Create256Hash();
            this.transactionList = new List<Transaction>();  
        }

        public Block(Block lastBlock, List<Transaction> TPool)
        {
            this.transactionList = new List<Transaction>();
            this.nonce = 0;
            this.timeStamp = DateTime.Now;
            this.index = lastBlock.index + 1;
            this.prevHash = lastBlock.hash;
            this.addFromPool(TPool, 2, "!");
        }
        // Constructor passed for index & hash of previous block
        public Block(int lastIndex, string lastHash) {
            this.nonce = 0;
            // new time
            this.timeStamp = DateTime.Now;
            // increment on last block
            this.index = lastIndex + 1;                         
            this.prevHash = lastHash;
            //Create hash from index, prevhash and time
            this.hash = this.Create256Hash();                              
        }

        // Constructor for not passed anything
        public Block() {
            // used for Genesis Block 
            this.transactionList = new List<Transaction>();
            // new time
            this.timeStamp = DateTime.Now;
            // First Block
            this.index = 0;
            // No Previous Hash
            this.prevHash = string.Empty;                                    
            this.hash = this.Create256Mine();
            // difficulty
            this.difficulty = 4;
        }

        public Block(Block lastBlock, List<Transaction> TPool, string MinerAddress, int setting, string address )
        {
            this.transactionList = new List<Transaction>();
            this.nonce = 0;
            this.timeStamp = DateTime.Now;
            this.difficulty = lastBlock.difficulty;
            // used to turn off adjustable difficulty
            this.adjustdiff(lastBlock.timeStamp); 
            this.index = lastBlock.index + 1;
            this.prevHash = lastBlock.hash;
            this.minerAddress = MinerAddress;
            // used to Create as well as append the reward transaction
            TPool.Add(createRewardTransaction(TPool)); 
            this.addFromPool(TPool, setting, address );
            // Calculates the merkle root 
            this.merkleRoot = MerkleRoot(transactionList); 

            if (THREADING == true)
            {
                this.ThreadedMine();
                this.hash = this.finalHash;
            }
            else this.hash = this.Create256Mine();
            Console.WriteLine("Here");
        }

        public override string ToString()
        {
            return ("\n\n\t\t[BLOCK START]"
                + "\nIndex: " + this.index
                + "\tTimestamp: " + this.timeStamp
                + "\nPrevious Hash: " + this.prevHash
                + "\n\t\t-- PoW --"
                + "\nDifficulty Level: " + this.difficulty
                + "\nNonce: " + this.nonce
                + "\nHash: " + this.hash + " " + this.finalHash
                + "\n\t\t-- Rewards --"
                + "\nReward: " + this.reward
                + "\nMiners Address: " + this.minerAddress
                + "\n\t\t-- " + this.transactionList.Count + " Transactions --"
                + "\nMerkle Root: " + this.merkleRoot
                + "\n" + String.Join("\n", this.transactionList)
                + "\n\t\t[BLOCK END]");

        }
        public string ReturnString() {
            return ("\n\n\t\t[BLOCK START]"
                + "\nIndex: " + this.index
                + "\tTimestamp: " + this.timeStamp
                + "\nPrevious Hash: " + this.prevHash
                + "\n\t\t-- PoW --"
                + "\nDifficulty Level: " + this.difficulty
                + "\nNonce: " + this.nonce
                + "\nHash: " + this.hash
                + "\n\t\t-- Rewards --"
                + "\nReward: " + this.reward
                + "\nMiners Address: " + this.minerAddress
                + "\n\t\t-- " + this.transactionList.Count + " Transactions --"
                + "\nMerkle Root: " + this.merkleRoot
                + "\n" + String.Join("\n", this.transactionList)
                + "\n\t\t[BLOCK END]");
        }

        public string readblock()
        {
            string s = "";
            s += this.ToString();

            foreach (Transaction T in transactionList)
            {
                s += ("\n" + T.ToString());
            }
            return s;

        }

        public void add2TList(Transaction T)
        {
            this.transactionList.Add(T);
        }

        public void addFromPool(List<Transaction> TP, int mode, string address)
        {
            int LIMIT = 5;
            int idx =0 ;

            
            while (transactionList.Count < LIMIT && TP.Count > 0 ) {
                // greedy
                if (mode == 0 ) {
                    for (int i = 0; ((i < TP.Count)); i++)
                    {
                        if (TP.ElementAt(i).Fee > TP.ElementAt(idx).Fee)
                        {
                            idx = i;
                        }
                    }
                    this.transactionList.Add(TP.ElementAt(idx));
                }
                // altruistic
                else if (mode == 1) {
                    for (int i = 0; ((i < TP.Count) && (i < LIMIT)); i++)
                    {
                        this.transactionList.Add(TP.ElementAt(i));
                    }
                }
                //random
                else if (mode == 2 ) {        
                    Random random = new Random();
                    idx = random.Next(0, TP.Count);
                    this.transactionList.Add(TP.ElementAt(idx));
                }       
                else if (mode == 3) {
                    for (int i = 0; i < TP.Count && (transactionList.Count < LIMIT); i++)
                    {                       
                        if (TP.ElementAt(i).SenderAddress == address)
                        {
                            this.transactionList.Add(TP.ElementAt(i));
                        }
                        else if (TP.ElementAt(i).RecipientAddress == address)
                        {
                            this.transactionList.Add(TP.ElementAt(i));
                        }
                        else
                        {
                            // Used to only take from this address
                        }                        
                    }
                    Console.WriteLine("Endless loop");
                }
                else
                // No Valid input so choose default --> Altruistic
                { 
                    mode = 1; 
                }
                TP = TP.Except(this.transactionList).ToList();     
            }

        }
 
        public string Create256Hash()
        { 
            SHA256 hasher;
            hasher = SHA256Managed.Create();
            String input = this.index.ToString() + this.timeStamp.ToString() + this.prevHash + this.nonce + this.merkleRoot + this.reward.ToString();
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes((input)));
            String hash = string.Empty;
            foreach (byte x in hashByte)
            {
                hash += String.Format("{0:x2}", x);
            }
            return hash;
        }

        private string Create256Mine()
        {             
            string hash =  "";          // could also do Createhash here, then no need to nonce--
            string diffString = new string('0', this.difficulty);
            while (hash.StartsWith(diffString)== false)
            {
                hash = this.Create256Hash();
                this.nonce++;
            }
            this.nonce--;
            if (hash is null){
                throw new IndexOutOfRangeException("No hash generated"); }
            return hash;
        }

        public static string MerkleRoot(List<Transaction> transactionList) {

            //list of transaction hashes for combining
            List<String> hashes = transactionList.Select(t => t.Hash).ToList(); 
            // Handle Blocks 
            if (hashes.Count == 0) // 0 = No transactions
            {
                return String.Empty;
            }
            // One transaction 
            else if (hashes.Count == 1) 
            {
                return HashCode.HashTools.combineHash(hashes[0], hashes[0]);
            }
            // Multiple transactions will Repeat until tree has been complete
            while (hashes.Count != 1) 
            {
                // Used to Keep track of current level
                List<String> merkleLeaves = new List<String>();
                // Steps over neighbouring pair and combines each
                for (int i = 0; i < hashes.Count; i += 2) 
                {
                    if (i == hashes.Count - 1)
                    {
                        // used for odd number of leaves
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i])); 
                    }
                    else
                    {
                        // used for Hashing neighbours leaves
                        merkleLeaves.Add(HashCode.HashTools.combineHash(hashes[i], hashes[i + 1])); 
                    }
                }
                // Updates the working layer
                hashes = merkleLeaves; 
            }
            return hashes[0]; // Return the root node
        }

        // Create reward for incentivising the mining of block
        public Transaction createRewardTransaction(List<Transaction> transactions)
        {
            // Sum of all transaction fees
            double fees = transactions.Aggregate(0.0, (acc, t) => acc + t.Fee);
            // reward as a transaction in the new block
            return new Transaction("Mine Rewards", "", minerAddress, (this.reward + fees), 0); 
        }

        private string ArchivedCreate256Hash() { 
            SHA256 hasher;
            hasher = SHA256Managed.Create();
            String input = this.index.ToString() + this.timeStamp.ToString() + this.prevHash;
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes((input)));
            String hash = string.Empty;

            foreach (byte x in hashByte)
            {
                hash += String.Format("{0:x2}", x);
            }
            return hash;
        }

        // For Multi Threading

        public void ThreadedMine(){
            Thread th1 = new Thread(this.Mine0);
            Thread th2 = new Thread(this.Mine1);

            th1.Start();
            th2.Start();

            while (th1.IsAlive == true || th2.IsAlive == true){Thread.Sleep(1);}

            if (this.finalHash1 is null) { 
                this.nonce = this.nonce0;
                this.finalHash = this.finalHash0;
            }
            else{
                this.nonce = this.nonce1;
                this.finalHash = this.finalHash1;
            }
            if (this.finalHash is null)
            {
                Console.WriteLine(this.ReturnString());
                throw new Exception("NULL finalhash" + 
                    " Nonce0: " + this.nonce0 + 
                    " Nonce1: "+ this.nonce1 + 
                    " Nonce: " + this.nonce +
                    " finalhash0 " + this.finalHash0 +
                    " finalhash1: " + this.finalHash1 +
                    " NewHash: " + this.Create256Hash()); 
            }
        }

        public void Mine0(){
            th1Fin = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Boolean check = false;
            String newHash;
            string diffString = new string('0', this.difficulty);

            while (check == false)
            {
                newHash = Create256Hash(this.nonce0);
                if (newHash.StartsWith(diffString) == true){
                    check = true;
                    this.finalHash0 = newHash;
                    Console.WriteLine("Block index: " + this.index);
                    Console.WriteLine("Thread 1 closed: Thread 1 found: " + this.finalHash0);
                    th1Fin = true;

                    Console.WriteLine(nonce0);
                    sw.Stop();
                    Console.WriteLine("Th1 mine:");
                    Console.WriteLine(sw.Elapsed);

                    return;
                }
                else if (th2Fin == true){
                    Console.WriteLine("Thread 1 closed: Thread 2 found: " + this.finalHash1 );
                    Thread.Sleep(1);
                    return;
                }
                else{
                    check = false;
                    this.nonce0 += 2;
                }
            }
            return;
        }

        public void Mine1()
        {
            th2Fin = false;
            Stopwatch sw = new Stopwatch();
            sw.Start();
            Boolean check = false;
            String newHash;
            string diffString = new string('0', this.difficulty);
            while (check == false){
                newHash = Create256Hash(this.nonce1);
                if (newHash.StartsWith(diffString) == true){
                    check = true;
                    this.finalHash1 = newHash;
                    Console.WriteLine("Block index: " + this.index);
                   Console.WriteLine("Thread 2 closed: Thread 2 found: " + this.finalHash1);
                    th2Fin = true;

                    Console.WriteLine(this.nonce1);
                    sw.Stop();
                    Console.WriteLine("Th2 mine:");
                    Console.WriteLine(sw.Elapsed);

                    return;
                }
                else if (th1Fin == true){
                    Console.WriteLine("Thread 2 closed: Thread 1 found: " + this.finalHash0);
                    Thread.Sleep(1);
                    return;
                }
                else{
                    check = false;
                    this.nonce1 += 2;
                }
            }
            return;
        }

        // nonce as a parameter
        public String Create256Hash(int inNonce)
        {
            SHA256 hasher;
            hasher = SHA256Managed.Create();
            String input = this.index.ToString() + this.timeStamp.ToString() + this.prevHash + inNonce + this.merkleRoot + this.reward.ToString();
            Byte[] hashByte = hasher.ComputeHash(Encoding.UTF8.GetBytes((input)));
            String hash = string.Empty;

            foreach (byte x in hashByte)
            {
                hash += String.Format("{0:x2}", x);
            }
            return hash;
        }

        //To adjust the difficulty
        public void adjustdiff(DateTime lastTime)
        {
            //elapsed time between now and the last block mined
            DateTime startTime = DateTime.UtcNow;
            TimeSpan timeDiff = startTime - lastTime;

            //If the difference is less than 5 seconds, the difficulty is increased to attempt to increase the time
            if (timeDiff < TimeSpan.FromSeconds(5))
            {
                this.difficulty++;
                Console.WriteLine("Time since last mine");
                Console.WriteLine(timeDiff);
                Console.WriteLine("New Difficulty:");
                Console.WriteLine(this.difficulty);
            }
            //If the difference is more than 5 seconds, the difficulty is increased to attempt to decrease the time
            else if (timeDiff > TimeSpan.FromSeconds(5))
            {
                difficulty--;
                Console.WriteLine("Time since last mine");
                Console.WriteLine(timeDiff);
                Console.WriteLine("New Difficulty:");
                Console.WriteLine(this.difficulty);
            }

            //Cap so that difficulty can never be higher than 5 or lower than 0
            if (this.difficulty <= 0)
            {
                this.difficulty = 0;
                Console.WriteLine("Difficulty too low, new difficulty:");
                Console.WriteLine(this.difficulty);
            }
            else if (this.difficulty >= 6)
            {
                this.difficulty = 4;
                Console.WriteLine("Difficulty too high, new difficulty:");
                Console.WriteLine(this.difficulty);
            }
        }

    }

}
