using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.XPath;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Reflection;
using System.Net.Sockets;
using System.Threading;

namespace CSGeneral
{
    /// <summary>
    /// General utility functions
    /// </summary>
    public class Utility{
        public static string EncodeBase64ToString(string base64String)
        {
            // Convert Base64 string back to a simmple string

            // No memos then exit
            if (base64String.Equals("")) return "";


            //Open up MemoryStream object to obtain an array of character bytes
            System.IO.MemoryStream mem = new System.IO.MemoryStream(
                  Convert.FromBase64String(base64String));

            string str = "";
            byte[] bite = mem.ToArray();

            // Loop through array adding each character byte to the end of a string
            foreach (byte abyte in bite)
                str += Convert.ToChar(abyte);

            //return formatted string
            return str;

        }

        public static string EncodeStringToBase64(string str)
        {
            // Converts given string to Base64

            System.IO.MemoryStream mem = new System.IO.MemoryStream(str.Length);
            StreamWriter Out = new StreamWriter(mem);
            Out.Write(str);

            // Loop through each character in the memo, writing each to a MemoryStream buffer
            //foreach (char character in str.ToCharArray())
            //    mem.WriteByte(Convert.ToByte(character));

            // convert byte array characters to Base64 return it.
            return Convert.ToBase64String(mem.GetBuffer());

        }

        public static Process RunProcess(string Executable, string Arguments, string JobFolder)
        {
            if (!File.Exists(Executable))
                throw new System.Exception("Cannot execute file: " + Executable + ". File not found.");
            Process PlugInProcess = new Process();
            PlugInProcess.StartInfo.FileName = Executable;
            PlugInProcess.StartInfo.Arguments = Arguments;
            // Determine whether or not the file is an executable; execute from the shell if it's not
            PlugInProcess.StartInfo.UseShellExecute = isManaged(Executable) == CompilationMode.Invalid;
            PlugInProcess.StartInfo.CreateNoWindow = true;
            if (!PlugInProcess.StartInfo.UseShellExecute)
            {
                PlugInProcess.StartInfo.RedirectStandardOutput = true;
                PlugInProcess.StartInfo.RedirectStandardError = true;
            }
            PlugInProcess.StartInfo.WorkingDirectory = JobFolder;
            PlugInProcess.Start();
            return PlugInProcess;
        }
        public static string CheckProcessExitedProperly(Process PlugInProcess)
        {
            if (!PlugInProcess.StartInfo.UseShellExecute)
            {
                string msg = "";
                if (PlugInProcess.StartInfo.RedirectStandardOutput)
                   msg = PlugInProcess.StandardOutput.ReadToEnd();
                PlugInProcess.WaitForExit();
                if (PlugInProcess.ExitCode != 0)
                {
                    if (PlugInProcess.StartInfo.RedirectStandardError)
                        msg += PlugInProcess.StandardError.ReadToEnd();
                    if (msg != "")
                        throw new System.Exception("Error from " + Path.GetFileName(PlugInProcess.StartInfo.FileName) + ": "
                                                                 + msg);
                }
                return msg;
            }
            else
                return "";
        }
        public static string ConvertURLToPath(string Url)
        {
            Uri uri = new Uri(Url);
            return uri.LocalPath;
        }

        public static void EnsureFileNameIsUnique(ref string FileName)
        {
            // -------------------------------------------------------------
            // Make sure the filename is unique. i.e. add a number to the end
            // to force uniqueness.
            // -------------------------------------------------------------
            string BaseName = Path.GetFileNameWithoutExtension(FileName);
            int Number = 1;
            while (File.Exists(FileName))
            {
                FileName = Path.Combine(Path.GetDirectoryName(FileName), BaseName + Number.ToString() + Path.GetExtension(FileName));
                Number++;
            }
            if (File.Exists(FileName))
                throw new Exception("Cannot find a unique filename for file: " + BaseName);
        }
        public static void DeleteFiles(string FileSpec, bool Recurse)
        {
            if (Directory.Exists(Path.GetDirectoryName(FileSpec)))
            {
                foreach (string FileName in Directory.GetFiles(Path.GetDirectoryName(FileSpec),
                                                               Path.GetFileName(FileSpec)))
                    File.Delete(FileName);
                if (Recurse)
                {
                    foreach (string SubDirectory in Directory.GetDirectories(Path.GetDirectoryName(FileSpec)))
                        DeleteFiles(Path.Combine(SubDirectory, Path.GetFileName(FileSpec)), true);
                }
            }
        }

        public static void FindFiles(string DirectoryName, string FileSpec, ref List<string> FileNames,
                                     bool recursive = false, bool SearchHiddenFolders = false)
        {
            foreach (string FileName in Directory.GetFiles(DirectoryName, FileSpec))
                FileNames.Add(FileName);
            if (recursive) 
               foreach (string ChildDirectoryName in Directory.GetDirectories(DirectoryName))
                  if (SearchHiddenFolders || (File.GetAttributes(ChildDirectoryName) & FileAttributes.Hidden) != FileAttributes.Hidden)
                     FindFiles(ChildDirectoryName, FileSpec, ref FileNames, recursive, SearchHiddenFolders);
        }

        /// <summary>
        /// Check for valid characters allowed in component names
        /// </summary>
        /// <param name="s">Test string</param>
        /// <returns>True if an invalid character is found</returns>
        public static bool CheckForInvalidChars(string s)
        {
            if ((s.Contains(",")) || (s.Contains(".")) || (s.Contains("/")) || (s.Contains("\\")) || (s.Contains("<")) || (s.Contains(">")) || (s.Contains("\"")) || (s.Contains("\'")) || (s.Contains("`")) || (s.Contains(":")) || (s.Contains("?")) || (s.Contains("|")) || (s.Contains("*")) || (s.Contains("&")) || (s.Contains("=")) || (s.Contains("!")))
            {
                return true;
            }
            else { return false; }


        }

        public static string FindFileOnPath(string FileName)
        {
            string PathVariable = Environment.GetEnvironmentVariable("PATH");
            if (PathVariable == null)
                throw new Exception("Cannot find PATH environment variable");
			string[] Paths;
			string PathSeparator;
			
			if (Path.VolumeSeparatorChar == '/') 
				PathSeparator = ":";
			else
				PathSeparator = ";";

			Paths = PathVariable.Split(PathSeparator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			
            foreach (string DirectoryName in Paths)
            {
                string FullPath = Path.Combine(DirectoryName, FileName);
                if (File.Exists(FullPath))
                    return FullPath;
            }
            return "";
        }

        /// <summary>
        /// Returns true if the specified type T is of type TypeName
        public static bool IsOfType(Type T, String TypeName)
        {
            while (T != null)
            {
                if (T.ToString() == TypeName)
                    return true;

                if (T.GetInterface(TypeName) != null)
                    return true;

                T = T.BaseType;
            }
            return false;
        }
        public enum CompilationMode
        {
            /// <summary>
            /// Unknown
            /// </summary>
            Invalid,
            /// <summary>
            /// Native Win32 code
            /// </summary>
            Native,
            /// <summary>
            /// Common Language Runtime
            /// </summary>
            CLR,
            /// <summary>
            /// Mixed mode
            /// </summary>
            Mixed
        };
        //=========================================================================
        /// <summary>
        /// Determine if the file refered to is a native win32 or a CLR assembly.
        /// Mixed mode assemblies are CLR.
        /// Visual C++ Developer Center. http://msdn2.microsoft.com/en-us/library/c91d4yzb(VS.80).aspx
        /// </summary>
        /// <param name="filename">File name of the Assembly or native dll to probe.</param>
        /// <returns>Compilation mode.</returns>
        //=========================================================================
        static public CompilationMode isManaged(string filename)
        {
            try
            {
                byte[] data = new byte[4096];
                FileInfo file = new FileInfo(filename);
                Stream fin = file.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                /*Int32 iRead =*/
                fin.Read(data, 0, 4096);
                fin.Close();

                // If we are running on Linux, the executable/so will start with the string 0x7f + 'ELF'
                // If the 5 byte is 1, it's a 32-bit image (2 indicates 64-bit)
                // If the 16th byte is 3, it's a shared object; 2 means it's an executable
                // If it's a Mono/.Net assembly, we should the the "Windows" header

                // If we're on Linux, see if it's a hash bang script. Should really
                // check executable flag via Mono.Unix.Native.Syscall.stat() too
                if (Path.VolumeSeparatorChar == '/' &&
                    Convert.ToChar(data[0]) == '#' &&
                    Convert.ToChar(data[1]) == '!')
                    return CompilationMode.Native;
                // For now, if we're on Linux just see if it has an "ELF" header
                if (Path.VolumeSeparatorChar == '/' && data[0] == 0x7f && data[1] == 'E' && data[2] == 'L' && data[3] == 'F')
                    return CompilationMode.Native;

                // Verify this is a executable/dll
                if (UInt16FromBytes(data, 0) != 0x5a4d)
                    return CompilationMode.Invalid;

                uint headerOffset = UInt32FromBytes(data, 0x3c);  // This will get the address for the WinNT header

                //at the file offset specified at offset 0x3c, is a 4-byte
                //signature that identifies the file as a PE format image file. This signature is �PE\0\0�
                if (UInt32FromBytes(data, headerOffset) != 0x00004550)
                    return CompilationMode.Invalid;

                //uint machineType = UInt16FromBytes(data, headerOffset + 4); //type of machine
                uint optionalHdrBase = headerOffset + 24;
                //uint exportTableAddr = UInt32FromBytes(data, optionalHdrBase + 96);     //.edata
                uint exportTableSize = UInt32FromBytes(data, optionalHdrBase + 96 + 4); //.edata size

                Int32 iLightningAddr = (int)headerOffset + 24 + 208;    //CLR runtime header addr & size
                Int32 iSum = 0;
                Int32 iTop = iLightningAddr + 8;

                for (int i = iLightningAddr; i < iTop; ++i)
                    iSum |= data[i];

                if (iSum == 0)
                    return CompilationMode.Native;
                else
                {
                    if (exportTableSize > 0)
                        return CompilationMode.Mixed;
                    else
                        return CompilationMode.CLR;
                }
            }
            catch (Exception e)
            {
                throw (e);
            }
        }
        static private UInt32 UInt32FromBytes(byte[] p, uint offset)
        {
            return (UInt32)(p[offset + 3] << 24 | p[offset + 2] << 16 | p[offset + 1] << 8 | p[offset]);
        }
        static private UInt16 UInt16FromBytes(byte[] p, uint offset)
        {
            return (UInt16)(p[offset + 1] << 8 | p[offset]);
        }

        /// <summary>
        /// Perform an insertion sort (stable sort) on the specified list.
        /// </summary>
        public static void StableSort<T>(IList<T> list, Comparison<T> comparison)
        {
            if (list == null)
                throw new ArgumentNullException("list");
            if (comparison == null)
                throw new ArgumentNullException("comparison");

            int count = list.Count;
            for (int j = 1; j < count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparison(list[i], key) > 0; i--)
                {
                    list[i + 1] = list[i];
                }
                list[i + 1] = key;
            }
        }


        /// <summary>
        /// Return all fields. The normal .NET reflection doesn't return private fields in base classes.
        /// This function does.
        /// </summary>
        public static List<FieldInfo> GetAllFields(Type type, BindingFlags flags)
        {
            if (type == typeof(Object)) return new List<FieldInfo>();

            var list = GetAllFields(type.BaseType, flags);
            // in order to avoid duplicates, force BindingFlags.DeclaredOnly
            list.AddRange(type.GetFields(flags | BindingFlags.DeclaredOnly));
            return list;
        }

        public static object GetValueOfFieldOrProperty(string Name, object Obj)
        {
                int Pos = Name.IndexOf('.');
            if (Pos > -1)
            {
                string FieldName = Name.Substring(0, Pos);
                Obj = GetValueOfFieldOrProperty(FieldName, Obj);
                if (Obj == null)
                    return null;
                else
                    return GetValueOfFieldOrProperty(Name.Substring(Pos + 1), Obj);
            }
            else
            {
                BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
                FieldInfo F = Obj.GetType().GetField(Name, Flags);
                if (F != null)
                    return F.GetValue(Obj);

                PropertyInfo P = Obj.GetType().GetProperty(Name, Flags);
                if (P != null)
                    return P.GetValue(Obj, null);

                return null;
            }
        }

        /// <summary>
        /// Trys to set the value of a public or private field or property. Name can have '.' characters. Will
        /// return true if successfull. Will throw if Value is the wrong type for the field
        /// or property. Supports strings/double/int conversion or direct setting.
        /// </summary>
        public static bool SetValueOfFieldOrProperty(string Name, object Obj, object Value)
        {
            if (Name.Contains("."))
            {
                int Pos = Name.IndexOf('.');
                string FieldName = Name.Substring(0, Pos);
                Obj = SetValueOfFieldOrProperty(FieldName, Obj, Value);
                if (Obj == null)
                    return false;
                else
                    return SetValueOfFieldOrProperty(Name.Substring(Pos + 1), Obj, Value);
            }
            else
            {
                BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;
                FieldInfo F = Obj.GetType().GetField(Name, Flags);
                if (F != null)
                {
                    if (F.FieldType == typeof(string))
                        F.SetValue(Obj, Value.ToString());
                    else if (F.FieldType == typeof(double))
                        F.SetValue(Obj, Convert.ToDouble(Value));
                    else if (F.FieldType == typeof(int))
                        F.SetValue(Obj, Convert.ToInt32(Value));
                    else
                        F.SetValue(Obj, Value);
                    return true;
                }

                PropertyInfo P = Obj.GetType().GetProperty(Name, Flags);
                if (P != null)
                {
                    if (P.PropertyType == typeof(string))
                        P.SetValue(Obj, Value.ToString(), null);
                    else if (P.PropertyType == typeof(double))
                        P.SetValue(Obj, Convert.ToDouble(Value), null);
                    else if (P.PropertyType == typeof(int))
                        P.SetValue(Obj, Convert.ToInt32(Value), null);
                    else
                        P.SetValue(Obj, Value, null);
                    return true;
                }

                return false;
            }
        }

        public class UploadFile
        {
            public string RemoteName;
            public string LocalName;
            public string ContentType;
        }
        /// <summary>
        ///  Upload a file 
        ///    http://dev.bratched.com/en/uploading-multiple-files-with-c/
        /// </summary>
        /// <param name="localFileName">Name of the file to be uploaded</param>
        /// <param name="username">remote username</param>
        /// <param name="password">remote password</param>
        /// <param name="hostname">remote hostname</param>
        /// <param name="remoteFileName">Full path and name of where the file goes</param>
        /// <returns></returns>
        static public byte[] UploadFiles(string address, IEnumerable<UploadFile> files, NameValueCollection formValues, string username, string password)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(address);
            request.SendChunked = true;

            string credentials = Convert.ToBase64String(ASCIIEncoding.ASCII.GetBytes(username + ":" + password));
            request.Headers.Add("Authorization", "Basic " + credentials);

            request.Method = "POST";
            var boundary = "---------------------------" + DateTime.Now.Ticks.ToString("x", NumberFormatInfo.InvariantInfo);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            boundary = "--" + boundary;
            using (var requestStream = request.GetRequestStream())
            {
                // Write the values
                foreach (string name in formValues.Keys)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"{1}{1}", name, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(formValues[name] + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                }

                // Write the files
                foreach (var file in files)
                {
                    var buffer = Encoding.ASCII.GetBytes(boundary + Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.UTF8.GetBytes(string.Format("Content-Disposition: form-data; name=\"{0}\"; filename=\"{1}\"{2}", file.RemoteName, file.LocalName, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    buffer = Encoding.ASCII.GetBytes(string.Format("Content-Type: {0}{1}{1}", file.ContentType, Environment.NewLine));
                    requestStream.Write(buffer, 0, buffer.Length);
                    Stream fileStream = File.Open(file.LocalName, FileMode.Open);
                    fileStream.CopyTo(requestStream);
                    fileStream.Close();
                    buffer = Encoding.ASCII.GetBytes(Environment.NewLine);
                    requestStream.Write(buffer, 0, buffer.Length);
                }

                var boundaryBuffer = Encoding.ASCII.GetBytes(boundary + "--");
                requestStream.Write(boundaryBuffer, 0, boundaryBuffer.Length);
            }

            using (var response = request.GetResponse())
            using (var responseStream = response.GetResponseStream())
            using (var stream = new MemoryStream())
            {
                responseStream.CopyTo(stream);
                return stream.ToArray();
            }
        }


        /// <summary>
        /// Look through the specified string for an environment variable name surrounded by
        /// % characters. Replace them with the environment variable value.
        /// </summary>
        public static string ReplaceEnvironmentVariables(string CommandLine)
        {
            if (CommandLine == null)
                return CommandLine;

            int PosPercent = CommandLine.IndexOf('%');
            while (PosPercent != -1)
            {
                string Value = null;
                int EndVariablePercent = CommandLine.IndexOf('%', PosPercent + 1);
                if (EndVariablePercent != -1)
                {
                    string VariableName = CommandLine.Substring(PosPercent + 1, EndVariablePercent - PosPercent - 1);
                    Value = System.Environment.GetEnvironmentVariable(VariableName);
                    if (Value == null)
                        Value = System.Environment.GetEnvironmentVariable(VariableName, EnvironmentVariableTarget.User);
                }

                if (Value != null)
                {
                    CommandLine = CommandLine.Remove(PosPercent, EndVariablePercent - PosPercent + 1);
                    CommandLine = CommandLine.Insert(PosPercent, Value);
                    PosPercent = PosPercent + 1;
                }

                else
                    PosPercent = PosPercent + 1;

                if (PosPercent >= CommandLine.Length)
                    PosPercent = -1;
                else
                    PosPercent = CommandLine.IndexOf('%', PosPercent);
            }
            return CommandLine;
        }


        /// <summary>
        /// Send a string to the specified socket server. Returns the response string. Will throw
        /// if cannot connect.
        /// </summary>
        public static string SocketSend(string ServerName, int Port, string Data)
        {
            string Response = null;
            TcpClient Server = null;
            try
            {
                Server = new TcpClient(ServerName, Convert.ToInt32(Port));
                Byte[] data = System.Text.Encoding.ASCII.GetBytes(Data);
                Server.GetStream().Write(data, 0, data.Length);

                Byte[] bytes = new Byte[8192];

                // Wait for data to become available.
                while (!Server.GetStream().DataAvailable)
                    Thread.Sleep(10);

                // Loop to receive all the data sent by the client.
                while (Server.GetStream().DataAvailable)
                {
                    int NumBytesRead = Server.GetStream().Read(bytes, 0, bytes.Length);
                    Response += System.Text.Encoding.ASCII.GetString(bytes, 0, NumBytesRead);
                }
            }
            finally
            {
                if (Server != null) {Server.GetStream ().Close (); Server.Close();}
            }
            return Response;
        }

        /// <summary>
        /// Store all macros found in the command line arguments. Macros are keyword = value
        /// </summary>
        public static Dictionary<string, string> ParseCommandLine(string[] args)
        {
            Dictionary<string, string> Options = new Dictionary<string, string>(StringComparer.CurrentCultureIgnoreCase);
            for (int i = 0; i < args.Length; i++)
            {
                StringCollection MacroBits = CSGeneral.StringManip.SplitStringHonouringQuotes(args[i], "=");
                if (MacroBits.Count == 2)
                    Options.Add(MacroBits[0].Replace("\"", ""), MacroBits[1].Replace("\"", ""));
            }
            return Options;
        }

		  /// <summary>
          /// Helper to return the real name of the file on disk (readlink() equivalent) - preserves
          /// upper/lower case
          /// </summary>
          public static string[] realNamesOfFiles(string filename)
          {
              string[] Files = null;
              if (Directory.Exists(filename))  // we've been given a directory name, not a file name; find everything in it.
              {
                  List<string> fileList = new List<string>();
                  Utility.FindFiles(filename, "*", ref fileList);
                  Files = fileList.ToArray();
              }
              else
              {
                  string dirName = Path.GetDirectoryName(filename);
                  if (String.IsNullOrEmpty(dirName))
                      dirName = Directory.GetCurrentDirectory();
                  if (Directory.Exists(dirName))
                  {
                      List<string> fileList = new List<string>();
                      Utility.FindFiles(dirName, Path.GetFileName(filename), ref fileList);
                      Files = fileList.ToArray();
                  }
              }
         if (Files != null)
             for (int i = 0; i < Files.Length; ++i)
                  Files[i] = Path.GetFullPath(Files[i].Replace("\"", ""));
         return Files; // probably undefined 
         }
    }
}
