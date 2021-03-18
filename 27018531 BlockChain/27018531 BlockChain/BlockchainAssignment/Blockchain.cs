using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlockchainAssignment
{
    class Blockchain
    {
        // Max number of transactions per block
        public int maxBlock { get => this.Blocks.Count; }
        // List of block objects forming the blockchain
        public List<Block> Blocks = new List<Block>();
        // List of pending transactions
        public List<Transaction> TransactionPool = new List<Transaction>();                

        public Blockchain()
        {
            Block genesis = new Block();
            Blocks.Add(genesis);
        }
        public string BlockString(int index)
        {
            return (Blocks.ElementAt(index).ReturnString());
        }

        public void add2TPool(Transaction Trans)
        {
            TransactionPool.Add(Trans);
        }
        public void add2Block(Block blck)
        {
            Blocks.Add(blck);
        }

        // Prints the block to UI
        public String GetBlockAsString(int index)
        {
            // Check if referenced block exists
            if (index >= 0 && index < Blocks.Count)
                // Return block as a string
                return Blocks[index].ToString(); 
            else
                return "No such block exists";
        }
        public void purgeTPool(List<Transaction> chosenT)
        {
            TransactionPool = TransactionPool.Except(chosenT).ToList();
        }
        public Block GetLastBlock()
        {
            return Blocks[Blocks.Count - 1];
        }

        //return to the transaction pool
        public List<Transaction> retTPool() {
            return TransactionPool;
        }

        //return to the blockchain
        public override string ToString()
        {
            return string.Join("\n", Blocks);
        }

        // Check validity of a blocks 
        public static bool ValidateHash(Block b){
            string rehash =string.Empty ;
            rehash = b.Create256Hash();
            Console.WriteLine("Rehash: " + rehash + " --> Hash: " + b.hash);
            return rehash.Equals(b.hash);
        }

        // Check the balance 
        public double GetBalance(String address)
        {
            // Accumulator value
            double balance = 0;

            // Looping through all approved transactions to assess account balance
            foreach (Block b in Blocks)
            {
                foreach (Transaction t in b.transactionList)
                {
                    if (t.RecipientAddress.Equals(address))
                    {
                        // Credit funds recieved
                        balance += t.Amount; 
                    }
                    if (t.SenderAddress.Equals(address))
                    {
                        // Debit payments placed
                        balance -= (t.Amount + t.Fee); 
                    }
                }
            }
            return balance;
        }

        // Check validity of the merkle root 
        public static bool ValidateMerkleRoot(Block b){
            String reMerkle = Block.MerkleRoot(b.transactionList);
            return reMerkle.Equals(b.merkleRoot);
        }
    }
}
