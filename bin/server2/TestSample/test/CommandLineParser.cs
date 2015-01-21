/*
 * Manual Page:
 * 
 * CommandLineParser cmdparser=new CommandLineParser();
 * cmdparser.Reset()           //Reset
 * cmdparser.SetOption('X')    //Set a certain option
 * cmdparser.X                 //Getter of option X
 * cmdparser.R                 //getter of option R
 * cmdparser.S                 //getter of option S
 * cmdparser.XMLName           //getter of XML file name to write
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 5  2014 created by Shikai Jin 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /*
    *Extract useful info from Command Line
    *indlucing options filepatttern, file path
    */
    public class CommandLineParser
    {
        string _XMLName = "output.xml";
        bool _X=false;
        bool _R=false;
        bool _S=false;
        string _path="";
        string _pattern="";

        public string XMLName { get { return _XMLName; } }
        public bool X { get { return _X; } }
        public bool R { get { return _R; } }
        public bool S { get { return _S; } }
        public string Path { get { return _path; } }
        public string Pattern { get { return _pattern; } }
        /*
        * Reset to original status.
        */
        void Reset()
        {
            _XMLName = "output.xml";
            _X = false;
            _R = false;
            _S = false;
            _path = "";
            _pattern = "";
        }
        /*
        * Detecting the option character
        */
        bool SetOption(char c)
        {
            switch (c)
            {

                case 'X':
                case 'x':
                    _X = true;
                    break;
                case 'R':
                case 'r':
                    _R = true;
                    break;
                case 'S':
                case 's':
                    _S = true;
                    break;
                default:
                    return false;
            }
            return true;
        }
        /*
        * Get the path, pattern, options.
        */
        public bool Parse(string[] tags)
        {
            if (tags.Length <= 2)
                return false;
            _path = tags[0];      //first one must be path
            _pattern = tags[1];  // second one must be pattern

            bool iswaitingforxmlpath = false;   // /X has not been detected
            for (int i = 2; i < tags.Length; i++)
            {
                // if /X has been detected, that indicates next token should be xml names 
                //or should be next option and xml with default name
                if (iswaitingforxmlpath == true)
                {                                 
                    if (tags[i][0] != '/')//is not /, which indicates a user defined XML name.
                    {
                        _XMLName = tags[i];
                        iswaitingforxmlpath = false;
                        continue;
                    }
                    else //no xml name detected, default name will be used.
                    {
                        iswaitingforxmlpath = false;
                    }
                }
                if (tags[i].Length == 2)//tokens like /X
                {
                    if (tags[i][0] == '/')
                    {
                        char o = tags[i][1];
                        if (o.ToString().ToLower() == "x")
                            iswaitingforxmlpath = true;
                        SetOption(o);
                    }
                }
                //tokens like /X/S. it's possible that user forgets to add a space between options
                if (tags[i].Length > 2)
                {
                    for (int j = 0; j < tags[i].Length; j++)
                    { 
                        if(j>=1)
                            if (tags[i][j - 1] == '/')
                            {
                                char o = tags[i][j];
                                if (o.ToString().ToLower() == "x")
                                    iswaitingforxmlpath = true;
                                SetOption(o);
                            }
                    }
                }
            }
            if (_X == true && _XMLName.Length == 0)
                return false;
            return true;
        }
    }
    class TestCMDParser
    { 
#if(TEST_COMMANDLINEPARSER)
        static void Show(string args)
        {
            CommandLineParser t = new CommandLineParser();
            string[] cmd = args.Split(' ');
            t.Parse(cmd);
            System.Console.WriteLine("CodeAnalyzer.exe "+args+":");
            System.Console.WriteLine("Path: " + t.Path);
            System.Console.WriteLine("PathPattern: " + t.Pattern);
            System.Console.WriteLine("Option X: " + (t.X ? "On" : "Off"));
            System.Console.WriteLine("Option R: " + (t.R ? "On" : "Off"));
            System.Console.WriteLine("Option S: " + (t.S ? "On" : "Off"));
        }
        static void Main(string[] args)
        {
            
            string cmdtext = "C:\\ *.cs /X /R";
            Show(cmdtext);
            System.Console.ReadLine();
        }
#endif
    }
}
