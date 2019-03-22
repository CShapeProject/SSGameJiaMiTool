using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace SSGameJiaMiTool
{
    public partial class SSGameJiaMiTool : Form
    {
        public SSGameJiaMiTool()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void CreateMiYaoBt_Click(object sender, EventArgs e)
        {
            string keyValue = ReadFromFileXml(GameJiaoYanKeyFile, "value"); //GameKey.db文件中的数据.
            if (keyValue == "")
            {
                //没有读到有效数据信息.
                MessageBox.Show("没有读到有效数据信息,请检查\"" + GameJiaoYanKeyFile + "\"文件是否正确!", "警告");
            }
            else
            {
                string jieMiKeyValue = Md5Decrypt(keyValue);
                string[] jieMiKyValArray = null;
                bool isMiYaoJieXiFailed = false;
                if (jieMiKeyValue != "")
                {
                    jieMiKyValArray = jieMiKeyValue.Split('#');
                    if (jieMiKyValArray.Length < 3)
                    {
                        isMiYaoJieXiFailed = true;
                    }
                    else
                    {
                        //秘钥解析成功.
                    }
                }
                else
                {
                    isMiYaoJieXiFailed = true;
                }

                if (isMiYaoJieXiFailed == true)
                {
                    //秘钥解析失败.
                    MessageBox.Show("秘钥数据解析失败,请检查\"" + GameJiaoYanKeyFile + "\"文件是否正确!", "警告");
                }
                else
                {
                    //秘钥解析成功.
                    //用Md5算法对数据进行加密.
                    string jiaoYanValue = Md5Encrypt(keyValue);
                    if (jiaoYanValue != "")
                    {
                        string fileName = jieMiKyValArray[2] + ".db";
                        string mac = jieMiKyValArray[0];
                        string time = jieMiKyValArray[1];
                        string gameName = jieMiKyValArray[2];
                        int recordGameCount = WriteGameInfoToFileXml(fileName, mac, time, gameName);
                        //保存加密后的数据到文件中.
                        WriteToFileXml(GameJiaoYanValueFile, "value", jiaoYanValue); //GameValue.db文件中的数据.
                        string msg = "秘钥产生成功,请查收\"" + GameJiaoYanValueFile + "\"文件!"
                            + "\nMac == " + mac                //电脑网卡地址.
                            + "\nTime == " + time              //秘钥创建时间.
                            + "\nGameName == " + gameName     //游戏名称.
                            + "\nrecordGameCount == " + recordGameCount; //已经注册的游戏数量.
                        MessageBox.Show(msg, "提示");
                    }
                    else
                    {
                        MessageBox.Show("秘钥产生失败,请检查\"" + GameJiaoYanKeyFile + "\"文件是否正确!", "警告");
                    }
                }
            }
        }

        #region 读写数据功能
        string GameJiaoYanKeyFile = "GameKey.db";
        string GameJiaoYanValueFile = "GameValue.db";
        public string ReadFromFileXml(string fileName, string attribute)
        {
            string filepath = fileName;
            string valueStr = "";
            if (File.Exists(filepath))
            {
                try
                {
                    XmlDocument xmlDoc = new XmlDocument();
                    xmlDoc.Load(filepath);
                    XmlNodeList nodeList = xmlDoc.SelectSingleNode("gameConfig").ChildNodes;
                    foreach (XmlElement xe in nodeList)
                    {
                        valueStr = xe.GetAttribute(attribute);
                    }
                    File.SetAttributes(filepath, FileAttributes.Normal);
                    xmlDoc.Save(filepath);
                }
                catch (Exception exception)
                {
                    File.SetAttributes(filepath, FileAttributes.Normal);
                    File.Delete(filepath);
                    MessageBox.Show("读取数据失败,请检查\"" + fileName + "\"文件是否正确!", "警告");
                    MessageBox.Show(exception.ToString(), "警告");
                }
            }
            return valueStr;
        }

        public void WriteToFileXml(string fileName, string attribute, string valueStr)
        {
            string filepath = fileName;
            //create file
            if (!File.Exists(filepath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("gameConfig");
                XmlElement elmNew = xmlDoc.CreateElement("config");

                root.AppendChild(elmNew);
                xmlDoc.AppendChild(root);
                xmlDoc.Save(filepath);
                File.SetAttributes(filepath, FileAttributes.Normal);
            }

            //update value
            if (File.Exists(filepath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filepath);
                XmlNodeList nodeList = xmlDoc.SelectSingleNode("gameConfig").ChildNodes;

                foreach (XmlElement xe in nodeList)
                {
                    xe.SetAttribute(attribute, valueStr);
                }
                File.SetAttributes(filepath, FileAttributes.Normal);
                xmlDoc.Save(filepath);
            }
        }
        
        public int WriteGameInfoToFileXml(string fileName, string mac, string time, string gameName)
        {
            int recordGameCount = 0; //已经注册的游戏数目.
            string filepath = fileName;
            string attMac = "Mac";
            string attTime = "Time";
            string attGameName = "GameName";
            string attGameCount = "GameCount";
            string eleConfig = "config";
            string eleGameInfo = "gameInfo";
            //create file
            if (!File.Exists(filepath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                XmlElement root = xmlDoc.CreateElement("gameConfig");
                XmlElement elmConfig = xmlDoc.CreateElement(eleConfig);
                root.AppendChild(elmConfig);
                //XmlElement elmNew = xmlDoc.CreateElement(eleGameInfo);
                //root.AppendChild(elmNew);
                xmlDoc.AppendChild(root);
                xmlDoc.Save(filepath);
                File.SetAttributes(filepath, FileAttributes.Normal);
            }

            //update value
            if (File.Exists(filepath))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(filepath);
                XmlNodeList nodeList = xmlDoc.SelectSingleNode("gameConfig").ChildNodes;
                recordGameCount = nodeList.Count;
                //是否需要新创建游戏配置信息.
                bool isCreateNewGameInfo = true;
                foreach (XmlElement xe in nodeList)
                {
                    //if (xe.Name == eleConfig)
                    //{
                    //    xe.SetAttribute(attGameName, gameName);
                    //    xe.SetAttribute(attGameCount, recordGameCount.ToString());
                    //}
                    if (xe.Name == eleGameInfo)
                    {
                        string macInfo = xe.GetAttribute(attMac);
                        if (macInfo == mac)
                        {
                            //游戏信息覆盖.
                            recordGameCount = nodeList.Count - 1;
                            xe.SetAttribute(attMac, mac);
                            xe.SetAttribute(attTime, time);
                            isCreateNewGameInfo = false;
                            break;
                        }
                    }
                }

                if (isCreateNewGameInfo == true)
                {
                    //新建游戏信息.
                    //XmlElement root = xmlDoc.GetElementById("gameConfig");
                    XmlElement root = xmlDoc.DocumentElement;
                    XmlElement elmNew = xmlDoc.CreateElement(eleGameInfo);
                    elmNew.SetAttribute(attMac, mac);
                    elmNew.SetAttribute(attTime, time);
                    root.AppendChild(elmNew);
                    xmlDoc.AppendChild(root);
                }

                foreach (XmlElement xe in nodeList)
                {
                    if (xe.Name == eleConfig)
                    {
                        //保存游戏注册的数量信息.
                        xe.SetAttribute(attGameName, gameName);
                        xe.SetAttribute(attGameCount, recordGameCount.ToString());
                        break;
                    }
                }
                File.SetAttributes(filepath, FileAttributes.Normal);
                xmlDoc.Save(filepath);
            }
            return recordGameCount;
        }
        #endregion

        #region MD5秘钥
        //建立加密对象的密钥和偏移量
        byte[] MD5_iv = { 102, 66, 93, 156, 78, 56, 253, 36 };//定义偏移量
        byte[] MD5_key = { 55, 36, 226, 128, 36, 99, 89, 39 };//定义密钥
        #endregion

        #region MD5加密
        /// <summary>   
        /// MD5加密   
        /// </summary>   
        /// <param name="strSource">需要加密的字符串</param>   
        /// <returns>MD5加密后的字符串</returns>   
        string Md5Encrypt(string strSource)
        {
            //把字符串放到byte数组中   
            byte[] bytIn = System.Text.Encoding.Default.GetBytes(strSource);
            //实例DES加密类   
            DESCryptoServiceProvider mobjCryptoService = new DESCryptoServiceProvider();
            mobjCryptoService.Key = MD5_iv;
            mobjCryptoService.IV = MD5_key;
            ICryptoTransform encrypto = mobjCryptoService.CreateEncryptor();
            //实例MemoryStream流加密密文件   
            System.IO.MemoryStream ms = new System.IO.MemoryStream();
            CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Write);
            cs.Write(bytIn, 0, bytIn.Length);
            cs.FlushFinalBlock();
            return System.Convert.ToBase64String(ms.ToArray());
        }
        #endregion

        #region MD5解密
        /// <summary>   
        /// MD5解密   
        /// </summary>   
        /// <param name="Source">需要解密的字符串</param>   
        /// <returns>MD5解密后的字符串</returns>   
        string Md5Decrypt(string Source)
        {
            string val = "0";
            try
            {
                //将解密字符串转换成字节数组   
                byte[] bytIn = System.Convert.FromBase64String(Source);
                //给出解密的密钥和偏移量，密钥和偏移量必须与加密时的密钥和偏移量相同   
                DESCryptoServiceProvider mobjCryptoService = new DESCryptoServiceProvider();
                mobjCryptoService.Key = MD5_iv;
                mobjCryptoService.IV = MD5_key;
                //实例流进行解密   
                System.IO.MemoryStream ms = new System.IO.MemoryStream(bytIn, 0, bytIn.Length);
                ICryptoTransform encrypto = mobjCryptoService.CreateDecryptor();
                CryptoStream cs = new CryptoStream(ms, encrypto, CryptoStreamMode.Read);
                StreamReader strd = new StreamReader(cs, Encoding.Default);
                val = strd.ReadToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show("数据解密失败,请检查\"" + Source + "\"数据是否正确!", "警告");
                MessageBox.Show(ex.ToString(), "警告");
            }
            return val;
        }
        #endregion
    }
}
