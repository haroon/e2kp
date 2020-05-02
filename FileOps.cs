using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Linq;


namespace e2kp
{
    class FileOps
    {
        static List<Category> categories = new List<Category>();

        public static void ProcessInputFile(string strEWalletFilePath, string strKeePassFilePath)
        {
            if (!File.Exists(strEWalletFilePath))
            {
                throw new FileNotFoundException(Constants.FileNotFoundExceptionDesc, strEWalletFilePath);
            }

            ReadEWalletFile(strEWalletFilePath);
            WriteKeePassFile(strKeePassFilePath);
        }

        static void ReadEWalletFile(string strEWalletFilePath)
        {
            bool bPrevLineIsBlank = true;

            using (StreamReader sr = File.OpenText(strEWalletFilePath))
            {
                Category current = null;
                StringBuilder sb = null;
                Card card = null;

                while (sr.EndOfStream == false)
                {
                    string line = sr.ReadLine();

                    if (line.StartsWith(Constants.Category))
                    {
                        if (current != null)
                        {
                            categories.Add(current);
                            current = null;
                        }
                        current = new Category();
                        current.Name = line.Substring(Constants.Category.Length).Trim();

                        // Must have empty line after `Category:`.
                        line = sr.ReadLine();
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            throw new FormatException(Constants.FileFormatExceptionDesc);
                        }
                    }
                    else if (current != null)
                    {
                        if (line.StartsWith(Constants.Card) && bPrevLineIsBlank)
                        {
                            bPrevLineIsBlank = false;
                            card = new Card();
                            card.Title = line.Substring(Constants.Card.Length).Trim();
                            sb = new StringBuilder();
                        }
                        else if (card != null)
                        {
                            if (line.StartsWith(Constants.UserName))
                            {
                                card.UserName = line.Substring(Constants.UserName.Length).Trim();
                            }
                            else if (line.StartsWith(Constants.Password))
                            {
                                card.Password = line.Substring(Constants.Password.Length).Trim();
                            }
                            else if (line.StartsWith(Constants.Url))
                            {
                                card.Url = line.Substring(Constants.Url.Length).Trim();
                            }
                            else if (!string.IsNullOrWhiteSpace(line))
                            {
                                if (line.StartsWith(Constants.CardNotes))
                                {
                                    sb.AppendLine(line);
                                    // Must have empty line after `Card Notes`.
                                    line = sr.ReadLine();
                                    if (!string.IsNullOrWhiteSpace(line))
                                    {
                                        throw new FormatException(Constants.FileFormatExceptionDesc);
                                    }
                                }
                                sb.AppendLine(line);
                            }
                            else
                            {
                                bPrevLineIsBlank = true;
                                if (card != null)
                                {
                                    card.Notes = sb.ToString();
                                    current.Cards.Add(card);
                                    sb = null;
                                    card = null;
                                }
                            }
                        }
                    }
                }

                if (current != null)
                {
                    categories.Add(current);
                    current = null;
                }
            }

        }
        
        static void WriteKeePassFile(string strKeePassFilePath)
        {
            XDocument xDoc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"));
            XElement xKeyPassFile = new XElement("KeePassFile");
            XElement xRoot = new XElement("Root");
            XElement xGroup = new XElement("Group",
                new XElement("UUID", CreateUUID()),
                new XElement("Name", "KeePass"),
                new XElement("Notes"));

            xDoc.Add(xKeyPassFile);
            xKeyPassFile.Add(xRoot);
            xRoot.Add(xGroup);

            foreach (var category in categories)
            {
                XElement xCategory = new XElement("Group",
                    new XElement("UUID", CreateUUID()),
                    new XElement("Name", category.Name)
                    );
                xGroup.Add(xCategory);
                AddCardsToXml(category, xCategory);
            }


            MemoryStream ms = new MemoryStream();
            var docXmlWriter = new System.Xml.XmlTextWriter(ms, System.Text.Encoding.UTF8);
            docXmlWriter.Formatting = System.Xml.Formatting.Indented;
            xDoc.WriteTo(docXmlWriter);
            docXmlWriter.Flush();
            ms.Flush();

            ms.Seek(0, SeekOrigin.Begin);

            StreamReader sr = new StreamReader(ms);
            string xmlResult = sr.ReadToEnd();

            File.WriteAllText(strKeePassFilePath, xmlResult);
        }
        
        static void AddCardsToXml(Category category, XElement xTop)
        {
            foreach (var card in category.Cards)
            {
                XElement xCard = new XElement("Entry",
                    new XElement("UUID", CreateUUID()),
                    new XElement("String", new XElement("Key", "Notes"), new XElement("Value", card.Notes)),
                    new XElement("String", new XElement("Key", "Password"), new XElement("Value", card.Password)),
                    new XElement("String", new XElement("Key", "Title"), new XElement("Value", card.Title)),
                    new XElement("String", new XElement("Key", "URL"), new XElement("Value", card.Url)),
                    new XElement("String", new XElement("Key", "UserName"), new XElement("Value", card.UserName))
                );

                XElement xAutoType = new XElement("AutoType",
                    new XElement("Enabled", "true"),
                    new XElement("DataTransferObfuscation", 0),
                    new XElement("Association",
                        new XElement("Window", "Target Window"),
                        new XElement("KeystrokeSequence", "{USERNAME}{TAB}{PASSWORD}{TAB}{ENTER}")
                            ),
                    new XElement("History")
                    );
                xCard.Add(xAutoType);

                xTop.Add(xCard);
            }
        }
        
        static string CreateUUID()
        {
            return Convert.ToBase64String(Guid.NewGuid().ToByteArray());
        }
    }
}
