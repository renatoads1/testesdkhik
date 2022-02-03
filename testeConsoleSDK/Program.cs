using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testeConsoleSDK
{
    class Program
    {
        private bool m_bInitSDK = false;
        private bool m_bRecord = false;
        private uint iLastErr = 0;
        private Int32 m_lUserID = -1;
        private Int32 m_lRealHandle = -1;
        private string str1;
        private string str2;
        private Int32 i = 0;
        private Int32 m_lTree = 0;
        private string str;
        private long iSelIndex = 0;
        private uint dwAChanTotalNum = 0;
        private uint dwDChanTotalNum = 0;
        private Int32 m_lPort = -1;
        private IntPtr m_ptrRealHandle;
        private int[] iIPDevID = new int[96];
        private int[] iChannelNum = new int[96];

        private CHCNetSDK.REALDATACALLBACK RealData = null;
        public CHCNetSDK.NET_DVR_DEVICEINFO_V30 DeviceInfo;
        public CHCNetSDK.NET_DVR_IPPARACFG_V40 m_struIpParaCfgV40;
        public CHCNetSDK.NET_DVR_STREAM_MODE m_struStreamMode;
        public CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo;
        public CHCNetSDK.NET_DVR_IPCHANINFO_V40 m_struChanInfoV40;
        private PlayCtrl.DECCBFUN m_fDisplayFun = null;
        public delegate void MyDebugInfo(string str);

        public frmRenato frmr = new frmRenato();

        public void Programs()
        {
            
            //inicializa chcnet
            m_bInitSDK = CHCNetSDK.NET_DVR_Init();

            if (m_bInitSDK == false)
            {
                //Console.WriteLine("NET_DVR_Init error!");
                return;
            }
            else
            {
                //保存SDK日志 To save the SDK log
                CHCNetSDK.NET_DVR_SetLogToFile(3, "C:\\SdkLog\\", true);

                //comboBoxView.SelectedIndex = 0;

                for (int i = 0; i < 64; i++)
                {
                    iIPDevID[i] = -1;
                    iChannelNum[i] = -1;
                }
                //Console.WriteLine("ok");
                
            }

        }

        static void Main(string[] args)
        {

            Program p = new Program();
            
            p.Programs();
            //faz login
            Console.WriteLine("escreva o ip do terminal ex: 192.168.0.1");
            string DVRIPAddressstr = Console.ReadLine();
            Console.WriteLine("escreva porta do terminal ex: 8000");
            string DVRPortNumberstr = Console.ReadLine();
            Console.WriteLine("escreva o usuario de acesso ex: admin");
            string DVRUserNamestr = Console.ReadLine();
            Console.WriteLine("escreva a senha do terminal ex: 123456");
            string DVRPasswordstr = Console.ReadLine();

            p.FazLoginServer(DVRIPAddressstr,DVRPortNumberstr, DVRUserNamestr,DVRPasswordstr);
            string varia = "";
            while (varia!="x")
            {
                Console.WriteLine("Para Live view (v) \npara Foto (f) \npara sair (x)  \n");
                varia = Console.ReadLine();
                if (varia == "v")
                {
                    p.btnPreview_Click();
                }
                else if (varia == "x")
                {
                    p.tiraFotoJpg();
                }
                else {
                    Environment.Exit(0);
                }
            }
        }

        //method que mostra os logs 
        public void DebugInfo(string str)
        {
            if (str.Length > 0)
            {
                str += "log:-> \n";
                //Console.WriteLine(str+" <-FimLog");
            }
        }

        public bool FazLoginServer(string DVRIPAddressstr,string DVRPortNumberstr,string DVRUserNamestr,string DVRPasswordstr)
        {
            if (m_lUserID < 0)
            {
                string DVRIPAddress = DVRIPAddressstr; //ip do server
                Int16 DVRPortNumber = Int16.Parse(DVRPortNumberstr);//porta do server
                string DVRUserName = DVRUserNamestr;//User Login
                string DVRPassword = DVRPasswordstr;//User Senha 
                m_lUserID = CHCNetSDK.NET_DVR_Login_V30(DVRIPAddress, DVRPortNumber, DVRUserName, DVRPassword, ref DeviceInfo);

                if (m_lUserID < 0)
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Login_V30 failed, error code= " + iLastErr; //登录失败，输出错误号 Failed to login and output the error code
                    DebugInfo(str);
                    return false;
                }
                else
                {
                    DebugInfo("NET_DVR_Login_V30 succ!");
                    //Console.WriteLine("Login ok na rede");
                    //?????????
                    dwAChanTotalNum = (uint)DeviceInfo.byChanNum;
                    dwDChanTotalNum = (uint)DeviceInfo.byIPChanNum + 256 * (uint)DeviceInfo.byHighDChanNum;
                    if (dwDChanTotalNum > 0)
                    {
                        InfoIPChannel();
                    }
                    else
                    {
                        for (i = 0; i < dwAChanTotalNum; i++)
                        {
                            ListAnalogChannel(i + 1, 1);
                            iChannelNum[i] = i + (int)DeviceInfo.byStartChan;
                            //Console.WriteLine($"Camera {iChannelNum[i].ToString()} Ligada");
                        }

                        //comboBoxView.SelectedItem = 1;
                        // MessageBox.Show("This device has no IP channel!");
                    }
                }

            }
            else
            {
                // Logout the device
                if (m_lRealHandle >= 0)
                {
                    DebugInfo("Please stop live view firstly"); // Stop live view before logout
                    return false;
                }

                if (!CHCNetSDK.NET_DVR_Logout(m_lUserID))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_Logout failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return false;
                }
                DebugInfo("NET_DVR_Logout succ!");
                //listViewIPChannel.Items.Clear();//Limpa checlist de canais
                m_lUserID = -1;
                //Console.WriteLine("Login");
            }
            return true;
        }

        //checa conexão
        public void InfoIPChannel()
        {
            uint dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40);

            IntPtr ptrIpParaCfgV40 = Marshal.AllocHGlobal((Int32)dwSize);
            Marshal.StructureToPtr(m_struIpParaCfgV40, ptrIpParaCfgV40, false);

            uint dwReturn = 0;
            int iGroupNo = 0;  
            
            //se falhar
            if (!CHCNetSDK.NET_DVR_GetDVRConfig(m_lUserID, CHCNetSDK.NET_DVR_GET_IPPARACFG_V40, iGroupNo, ptrIpParaCfgV40, dwSize, ref dwReturn))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_GET_IPPARACFG_V40 failed, error code= " + iLastErr;
                //erro nos dados de conexão
                DebugInfo(str);
            }
            else
            {
                DebugInfo("NET_DVR_GET_IPPARACFG_V40 succ! conectou aueheuahaeuheau!!!!!!");

                m_struIpParaCfgV40 = (CHCNetSDK.NET_DVR_IPPARACFG_V40)Marshal.PtrToStructure(ptrIpParaCfgV40, typeof(CHCNetSDK.NET_DVR_IPPARACFG_V40));

                for (i = 0; i < dwAChanTotalNum; i++)
                {
                    ListAnalogChannel(i + 1, m_struIpParaCfgV40.byAnalogChanEnable[i]);
                    iChannelNum[i] = i + (int)DeviceInfo.byStartChan;
                }

                byte byStreamType = 0;
                uint iDChanNum = 64;

                if (dwDChanTotalNum < 64)
                {
                    iDChanNum = dwDChanTotalNum; 
                    //如果设备IP通道小于64路，按实际路数获取
                }

                for (i = 0; i < iDChanNum; i++)
                {
                    iChannelNum[i + dwAChanTotalNum] = i + (int)m_struIpParaCfgV40.dwStartDChan;
                    byStreamType = m_struIpParaCfgV40.struStreamMode[i].byGetStreamType;

                    dwSize = (uint)Marshal.SizeOf(m_struIpParaCfgV40.struStreamMode[i].uGetStream);
                    switch (byStreamType)
                    {
                        //目前NVR仅支持直接从设备取流 NVR supports only the mode: get stream from device directly
                        case 0:
                            IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwSize);
                            Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfo, false);
                            m_struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));

                            //列出IP通道 List the IP channel
                            ListIPChannel(i + 1, m_struChanInfo.byEnable, m_struChanInfo.byIPID);
                            iIPDevID[i] = m_struChanInfo.byIPID + m_struChanInfo.byIPIDHigh * 256 - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfo);
                            break;
                        case 6:
                            IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal((Int32)dwSize);
                            Marshal.StructureToPtr(m_struIpParaCfgV40.struStreamMode[i].uGetStream, ptrChanInfoV40, false);
                            m_struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));

                            //列出IP通道 List the IP channel
                            ListIPChannel(i + 1, m_struChanInfoV40.byEnable, m_struChanInfoV40.wIPID);
                            iIPDevID[i] = m_struChanInfoV40.wIPID - iGroupNo * 64 - 1;

                            Marshal.FreeHGlobal(ptrChanInfoV40);
                            break;
                        default:
                            break;
                    }
                }
            }
            Marshal.FreeHGlobal(ptrIpParaCfgV40);

        }//fim fecha conexao

        //mais uma função sem 
        public void ListAnalogChannel(Int32 iChanNo, byte byEnable)
        {
            str1 = String.Format("Camera {0}", iChanNo);
            m_lTree++;

            if (byEnable == 0)
            {
                str2 = "Disabled"; // This channel has been disabled               
            }
            else
            {
                str2 = "Enabled"; // This channel has been enabled
            }
            //Console.WriteLine("result linha 247 -> "+str2);
            //listViewIPChannel.Items.Add(new ListViewItem(new string[] { str1, str2 }));//将通道添加到列表中 add the channel to the list
        }

        //mais um method
        public void ListIPChannel(Int32 iChanNo, byte byOnline, int byIPID)
        {
            str1 = String.Format("IPCamera {0}", iChanNo);
            m_lTree++;

            if (byIPID == 0)
            {
                str2 = "X"; //通道空闲，没有添加前端设备 the channel is idle                  
            }
            else
            {
                if (byOnline == 0)
                {
                    str2 = "offline"; //通道不在线 the channel is off-line
                }
                else
                    str2 = "online"; //通道在线 The channel is on-line
            }

           // listViewIPChannel.Items.Add(new ListViewItem(new string[] { str1, str2 }));//将通道添加到列表中 add the channel to the list
        }
        //tira foto jpg
        public void tiraFotoJpg()
        {
            int lChannel = iChannelNum[(int)iSelIndex]; //pega o canal da camera conectada
            //instancia um objeto pegajpg
            CHCNetSDK.NET_DVR_JPEGPARA lpJpegPara = new CHCNetSDK.NET_DVR_JPEGPARA();
            lpJpegPara.wPicQuality = 0; //图像质量 Image quality
            lpJpegPara.wPicSize = 0xff; //Resolução da captura de tela Picture size: 0xff-Auto(Use a resolução de fluxo atual) 
            //A resolução de captura precisa ser compatível com o dispositivo. Para obter mais valores, consulte a documentação do SDK

            //JPEG Os instantâneos são salvos como arquivos Capture a JPEG picture
            string sJpegPicFileName;
            sJpegPicFileName = "filetest2.jpg";//Caminho de salvamento da imagem e nome do arquivo 
            //em caso de erro
            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture(m_lUserID, lChannel, ref lpJpegPara, sJpegPicFileName))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture failed, error code= " + iLastErr;
                DebugInfo(str);
                return;
            }
            else
            {
                str = "NET_DVR_CaptureJPEGPicture succ and the saved file is " + sJpegPicFileName;
                DebugInfo(str);
            }

            //JEPG instantâneo，Os dados são salvos no buffer 
            uint iBuffSize = 400000; //The buffer size should not be less than the picture size
            byte[] byJpegPicBuffer = new byte[iBuffSize];
            uint dwSizeReturned = 0;

            if (!CHCNetSDK.NET_DVR_CaptureJPEGPicture_NEW(m_lUserID, lChannel, ref lpJpegPara, byJpegPicBuffer, iBuffSize, ref dwSizeReturned))
            {
                iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                str = "NET_DVR_CaptureJPEGPicture_NEW failed, error code= " + iLastErr;
                DebugInfo(str);
                return;
            }
            else
            {
               
                    // save the data into a file
                    string str = "buffertest.jpg";
                    FileStream fs = new FileStream(str, FileMode.Create);
                    int iLen = (int)dwSizeReturned;
                    fs.Write(byJpegPicBuffer, 0, iLen);
                    fs.Close();
                    Process.Start(str);
                    //Thread.Sleep(2);
              
                
                str = "NET_DVR_CaptureJPEGPicture_NEW succ and save the data in buffer to 'buffertest.jpg'.";
                DebugInfo(str);
            }

            return;
        }

        //ver em tempo real
        public void btnPreview_Click()
        {
            if (m_lUserID < 0)
            {
                //Console.WriteLine("você deve fazer o lgin primeiro seu animal!!! =]]]");
                return;
            }

            if (m_bRecord)
            {
                //Console.WriteLine("Pare a Gravação Primeiro");
                return;
            }

            if (m_lRealHandle < 0)
            {//entra
                CHCNetSDK.NET_DVR_PREVIEWINFO lpPreviewInfo = new CHCNetSDK.NET_DVR_PREVIEWINFO();
                lpPreviewInfo.hPlayWnd = frmr.ptbRenato.Handle;//Janela de visualização 
                lpPreviewInfo.lChannel = iChannelNum[(int)iSelIndex];// 预览的设备通道 the device channel number
                lpPreviewInfo.dwStreamType = 0;//码流类型：0-主码流，1-子码流，2-码流3，3-码流4，以此类推
                lpPreviewInfo.dwLinkMode = 0;//连接方式：0- TCP方式，1- UDP方式，2- 多播方式，3- RTP方式，4-RTP/RTSP，5-RSTP/HTTP 
                lpPreviewInfo.bBlocked = true; //0- 非阻塞取流，1- 阻塞取流
                lpPreviewInfo.dwDisplayBufNum = 15; //播放库显示缓冲区最大帧数
                
                IntPtr pUser = IntPtr.Zero;//用户数据 user data 

                if (true)
                {//entra
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, null/*RealData*/, pUser);
                }
                else
                {
                    lpPreviewInfo.hPlayWnd = IntPtr.Zero;// live view window
                    m_ptrRealHandle = frmr.ptbRenato.Handle;
                    RealData = new CHCNetSDK.REALDATACALLBACK(RealDataCallBack);//预览实时流回调函数 real-time stream callback function 
                    m_lRealHandle = CHCNetSDK.NET_DVR_RealPlay_V40(m_lUserID, ref lpPreviewInfo, RealData, pUser);
                }
                
                if (false)
                {
                    
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_RealPlay_V40 failed, error code= " + iLastErr; //failed to start live view, and output the error code.
                    DebugInfo(str);

                }
                else
                {//entra
                    DebugInfo("NET_DVR_RealPlay_V40 succ!");
                    frmr.ShowDialog();
                }
            }
            else
            {
                // Stop live view 
                if (!CHCNetSDK.NET_DVR_StopRealPlay(m_lRealHandle))
                {
                    iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                    str = "NET_DVR_StopRealPlay failed, error code= " + iLastErr;
                    DebugInfo(str);
                    return;
                }

                if ((m_lPort >= 0))
                {
                    if (!PlayCtrl.PlayM4_Stop(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_Stop failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    if (!PlayCtrl.PlayM4_CloseStream(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_CloseStream failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    if (!PlayCtrl.PlayM4_FreePort(m_lPort))
                    {
                        iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                        str = "PlayM4_FreePort failed, error code= " + iLastErr;
                        DebugInfo(str);
                    }
                    m_lPort = -1;
                }
                //CHAMA FORMULÁRIO
                DebugInfo("NET_DVR_StopRealPlay succ!");
                m_lRealHandle = -1;
                frmr.ptbRenato.Invalidate();//refresh the window
            }
            return;
        }

        //calback
        public void RealDataCallBack(Int32 lRealHandle, UInt32 dwDataType, IntPtr pBuffer, UInt32 dwBufSize, IntPtr pUser)
        {
            //下面数据处理建议使用委托的方式
            MyDebugInfo AlarmInfo = new MyDebugInfo(DebugInfo);
            switch (dwDataType)
            {
                case CHCNetSDK.NET_DVR_SYSHEAD:     // sys head
                    if (dwBufSize > 0)
                    {
                        if (m_lPort >= 0)
                        {
                            return; //同一路码流不需要多次调用开流接口
                        }

                        //获取播放句柄 Get the port to play
                        if (!PlayCtrl.PlayM4_GetPort(ref m_lPort))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_GetPort failed, error code= " + iLastErr;
                            this.frmr.BeginInvoke(AlarmInfo, str);
                            break;
                        }

                        //设置流播放模式 Set the stream mode: real-time stream mode
                        if (!PlayCtrl.PlayM4_SetStreamOpenMode(m_lPort, PlayCtrl.STREAME_REALTIME))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "Set STREAME_REALTIME mode failed, error code= " + iLastErr;
                            this.frmr.BeginInvoke(AlarmInfo, str);
                        }

                        //打开码流，送入头数据 Open stream
                        if (!PlayCtrl.PlayM4_OpenStream(m_lPort, pBuffer, dwBufSize, 2 * 1024 * 1024))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_OpenStream failed, error code= " + iLastErr;
                            this.frmr.BeginInvoke(AlarmInfo, str);
                            break;
                        }


                        // Set the display buffer number
                        if (!PlayCtrl.PlayM4_SetDisplayBuf(m_lPort, 15))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_SetDisplayBuf failed, error code= " + iLastErr;
                            this.frmr.BeginInvoke(AlarmInfo, str);
                        }

                        // Set the display mode
                        if (!PlayCtrl.PlayM4_SetOverlayMode(m_lPort, 0, 0/* COLORREF(0)*/)) //play off screen 
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_SetOverlayMode failed, error code= " + iLastErr;
                            this.frmr.BeginInvoke(AlarmInfo, str);
                        }

                        // Set callback function of decoded data
                        m_fDisplayFun = new PlayCtrl.DECCBFUN(DecCallbackFUN);
                        if (!PlayCtrl.PlayM4_SetDecCallBackEx(m_lPort, m_fDisplayFun, IntPtr.Zero, 0))
                        {
                            this.frmr.BeginInvoke(AlarmInfo, "PlayM4_SetDisplayCallBack fail");
                        }

                        // Start to play                       
                        if (!PlayCtrl.PlayM4_Play(m_lPort, m_ptrRealHandle))
                        {
                            iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                            str = "PlayM4_Play failed, error code= " + iLastErr;
                            this.frmr.BeginInvoke(AlarmInfo, str);
                            break;
                        }
                    }
                    break;
                case CHCNetSDK.NET_DVR_STREAMDATA:     // video stream data
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        for (int i = 0; i < 999; i++)
                        {
                            // Input the stream data to decode
                            if (!PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                                str = "PlayM4_InputData failed, error code= " + iLastErr;
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
                default:
                    if (dwBufSize > 0 && m_lPort != -1)
                    {
                        // Input the other data
                        for (int i = 0; i < 999; i++)
                        {
                            if (!PlayCtrl.PlayM4_InputData(m_lPort, pBuffer, dwBufSize))
                            {
                                iLastErr = PlayCtrl.PlayM4_GetLastError(m_lPort);
                                str = "PlayM4_InputData failed, error code= " + iLastErr;
                                Thread.Sleep(2);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                    break;
            }
        }
        private void DecCallbackFUN(int nPort, IntPtr pBuf, int nSize, ref PlayCtrl.FRAME_INFO pFrameInfo, int nReserved1, int nReserved2)
        {
            // 将pBuf解码后视频输入写入文件中（解码后YUV数据量极大，尤其是高清码流，不建议在回调函数中处理）
            if (pFrameInfo.nType == 3) //#define T_YV12	3
            {
                //    FileStream fs = null;
                //    BinaryWriter bw = null;
                //    try
                //    {
                //        fs = new FileStream("DecodedVideo.yuv", FileMode.Append);
                //        bw = new BinaryWriter(fs);
                //        byte[] byteBuf = new byte[nSize];
                //        Marshal.Copy(pBuf, byteBuf, 0, nSize);
                //        bw.Write(byteBuf);
                //        bw.Flush();
                //    }
                //    catch (System.Exception ex)
                //    {
                //        MessageBox.Show(ex.ToString());
                //    }
                //    finally
                //    {
                //        bw.Close();
                //        fs.Close();
                //    }
            }
        }


    }
}
