using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using ContactLibrary;

namespace ContactDataProviders
{
    public class XmlContactDAL : IContactDAL
    {
        const string CACHE_KEY = "GetContacts";

        private Random random = new Random((int)DateTime.Now.Ticks);//thanks to McAden

        private bool InWebApplication { get { return (System.Web.HttpContext.Current != null); } }
        private static Dictionary<string, object> _appCache = new Dictionary<string,object>();

        private string RandomString(int size)
        {
            StringBuilder builder = new StringBuilder();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 *random.NextDouble() + 65)));
                builder.Append(ch);
            }

            return builder.ToString();
        }


        List<Contact> _data
        {
            get
            {
                

                List<Contact> contacts;

                if (InWebApplication)
                    contacts = HttpContext.Current.Cache[CACHE_KEY] as List<Contact>;
                else
                    contacts = (_appCache.ContainsKey(CACHE_KEY)) ? _appCache[CACHE_KEY] as List<Contact> : null;

                if (contacts == null)
                {
                    contacts = new List<Contact>();
                    for (var i = 1; i <= 500; i++)
                    {
                        var firstName = RandomString(4);
                        var lastName = RandomString(6);
                        string email = String.Format("{0}{1}@gmail.com", firstName, lastName);

                        var counter = 0;
                        while (contacts.Count(c => c.Email == email) > 0)
                        {
                            counter++;
                            email = String.Format("{0}{1}{2}@gmail.com", firstName, lastName, counter);
                        }

                        contacts.Add(
                            new Contact
                            {
                                FirstName = firstName,
                                LastName = lastName,
                                Id = i,
                                Email = email
                            }
                        );
                    }

                }

                if (InWebApplication)
                    HttpContext.Current.Cache[CACHE_KEY] = contacts;
                else
                    _appCache[CACHE_KEY] = contacts;

                return contacts;
            }
        }

        private string XmlFile
        {
            get
            {
                if (InWebApplication)
                    return System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Contacts.xml");
                else
                {
                    string currentDirectory = System.IO.Directory.GetCurrentDirectory();
                    
                    if(currentDirectory.Contains("ContactConsole")) currentDirectory = currentDirectory.Substring(0, currentDirectory.LastIndexOf("ContactConsole"));
                    return String.Format("{0}web\\App_Data\\Contacts.xml", currentDirectory);
                }
            }
        }





        private void seedData()
        {
            if (File.Exists(XmlFile))
                return;
            
            var xdoc = new XDocument(
                new XElement("Contacts")    
            );
            foreach (var contact in _data)
            {
                xdoc.Descendants("Contacts").First().Add(MapFromContact(contact));
            }
            xdoc.Save(XmlFile);
            
        }

        XElement MapFromContact(Contact contact)
        {
            var elem =  new XElement(
                    "Contact",
                    new XAttribute("Id", contact.Id),
                    new XElement("FirstName", contact.FirstName),
                    new XElement("LastName", contact.LastName),
                    new XElement("Email", contact.Email)
                );

                var nicknames = new XElement("Nicknames");
                foreach (var nickname in contact.Nicknames)
                    nicknames.Add(new XElement("Nickname", nickname));
                elem.Add(nicknames);


            return elem;
        }

        Contact MapToContact(XElement node)
        {
            return new Contact
                {
                    Id = (int)node.Attribute("Id"),
                    FirstName = node.Element("FirstName").Value,
                    LastName = node.Element("LastName").Value,
                    Email = node.Element("Email").Value,
                    Nicknames = node.Descendants("Nickname").Select(n=>n.Value).ToList()

                };
        }

        public void Insert(Contact contact)
        {
            var fileName = System.Web.HttpContext.Current.Server.MapPath("~/App_Data/Contacts.xml");
            var xdoc = XDocument.Load(fileName);
            contact.Id = xdoc.Descendants("Contact").Max(node=> (int)node.Attribute("Id")) + 1;
            xdoc.Descendants("Contacts").First().Add(MapFromContact(contact));
            xdoc.Save(fileName);
        }

        private XElement GetContactNode(XDocument doc, Contact contact)
        {
            return doc.Descendants("Contact").Single(node => node.Attribute("Id").Value == contact.Id.ToString());
        }

        public void Edit(Contact contact)
        {
            var doc = XDocument.Load(XmlFile);
            var nodeToReplace = GetContactNode(doc, contact);
            nodeToReplace.ReplaceWith(MapFromContact(contact));
            doc.Save(XmlFile);
        }

        public void Delete(Contact contact)
        {
            var doc = XDocument.Load(XmlFile);
            var nodeToDelete = GetContactNode(doc, contact);
            nodeToDelete.Remove();
            doc.Save(XmlFile);
        }

        public List<Contact> GetContacts()
        {
            seedData();
            var results = new List<Contact>();
            var xdoc = XDocument.Load(XmlFile);
            foreach (var node in xdoc.Document.Element("Contacts").Elements("Contact"))
            {
                results.Add(MapToContact(node));
            }
            return results;
        }

        public Contact GetContact(int id)
        {
            var xdoc = XDocument.Load(XmlFile);
            var contact = new Contact {Id = id };
            var node = GetContactNode(xdoc, contact);
            return MapToContact(node);
        }
    }
}