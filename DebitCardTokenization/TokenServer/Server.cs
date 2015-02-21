﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.Xml.Serialization;
using Tokenization.Consts;

namespace TokenServer
{
    public class Server
    {
        private Thread readThread;
        private List<Client> clients;
        private List<BankCard> bankCards;
        private Action<object> DisplayMethod;
        private Action<object> DisplayError;

        public Server(Action<object> message, Action<object> error)
        {
            DisplayMethod = message;
            DisplayError = error;
            Load();
        }

        ~Server()
        {
            Serialize();
        }

        public void Load()
        {
            clients = new List<Client>();
            bankCards = new List<BankCard>();

            readThread = new Thread(new ThreadStart(RunServer));
            readThread.Start();
            Deserialize();
        }

        private void RunServer()
        {
            TcpListener listener;
            try
            {
                IPAddress localhost = IPAddress.Parse(Constants.LOCALHOST);
                listener = new TcpListener(localhost, Constants.PORT);

                listener.Start();
                DisplayMethod(Constants.WAITING_FOR_CONNECTION);

                while (true)
                {
                    ThreadPool.QueueUserWorkItem(new WaitCallback(new ClientProcessor(
                                                 DisplayMethod, clients, bankCards).Process),
                                                 listener.AcceptSocket());

                    DisplayMethod(Constants.CONNECTION_ACCEPTED);
                }
            }
            catch (Exception e)
            {
                DisplayError(e.Message);
            }
        }

        public void Serialize()
        {
            try
            {
                XmlSerializer cardSer = new XmlSerializer(typeof(List<BankCard>));
                XmlSerializer userSer = new XmlSerializer(typeof(List<Client>));
                using (FileStream cards = new FileStream(Constants.CARDS_FILE, FileMode.Create, FileAccess.Write))
                using (FileStream users = new FileStream(Constants.USERS_FILE, FileMode.Create, FileAccess.Write))
                {

                    cardSer.Serialize(cards, bankCards);
                    userSer.Serialize(users, clients);
                }
            }
            catch (Exception e)
            {
                DisplayMethod(e.Message);
            }
        }

        private void Deserialize()
        {
            try
            {
                XmlSerializer cardDes = new XmlSerializer(typeof(List<BankCard>));
                XmlSerializer userDes = new XmlSerializer(typeof(List<Client>));
                using (FileStream cards = new FileStream(Constants.CARDS_FILE, FileMode.Open, FileAccess.Read))
                using (FileStream users = new FileStream(Constants.USERS_FILE, FileMode.Open, FileAccess.Read))
                {

                    bankCards = (List<BankCard>)cardDes.Deserialize(cards);
                    clients = (List<Client>)userDes.Deserialize(users);
                }
                DisplayMethod(String.Format(
                    Constants.DESERIALIZE_SUCCESSFUL,
                    bankCards.Count, clients.Count));
            }
            catch(Exception e)
            {
                DisplayMethod(e.Message);
                bankCards = new List<BankCard>();
                clients = new List<Client>();
                DisplayMethod(Constants.LISTS_RESET);
            }
        }

        public void ExportSortedByCard(Action<object> DialogShower)
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            DialogShower(dialog);

            FileStream output = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(output);

            IEnumerable<BankCard> sorted = bankCards.OrderBy(card => card.ID);
            foreach (BankCard card in sorted)
            {
                foreach (Token token in card.Tokens)
                    writer.WriteLine(String.Format(Constants.CARD_VS_TOKEN, card.ID, token.ID));
            }
            writer.Close();
            output.Close();
            
        }

        public void ExportSortedByToken(Action<object> DialogShower)
        {
            System.Windows.Forms.SaveFileDialog dialog = new System.Windows.Forms.SaveFileDialog();
            DialogShower(dialog);

            FileStream output = new FileStream(dialog.FileName, FileMode.Create, FileAccess.Write);
            StreamWriter writer = new StreamWriter(output);

            List<Token> tokens = new List<Token>();

            foreach (BankCard card in bankCards)
            {
                foreach (Token token in card.Tokens)
                    tokens.Add(token);
            }
            IEnumerable<Token> sorted = tokens.OrderBy(token => token.ID);

            foreach (Token token in sorted)
                writer.WriteLine(String.Format(Constants.TOKEN_VS_CARD, token.ID, token.Owner));

            writer.Close();
            output.Close();
        }
    }
}
