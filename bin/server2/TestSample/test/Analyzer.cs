/*
 * Manual Page:
 * Analyzer anal=new Analyzer();
 * anal.start(args)               //pass the arg in
 * 
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 5  2014 
 * created by Shikai Jin 
 * 
 * Ver 2.0  Oct. 6 2014 
 * class Analyzer 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /*
    * Controller of analysis process
    */
    public class Analyzer
    {
        /*
        * Start the process of analysis
        */
        public bool Start(String[] args)
        {
            /*
             * Parse CMD
             *  
             */            
            CommandLineParser cmdp=new CommandLineParser();
            if (false == cmdp.Parse(args))
            {
                System.Console.WriteLine("USAGE:codeanalyzer filepath filepattern [/R][/X][/S]");
                return false;
            }
            /*
             * 
             * Parse code 
             */
            Parser codep = new Parser();
            try
            {
                codep.Parse(cmdp.Path, cmdp.Pattern, cmdp.S);
            }
            catch 
            {
                System.Console.WriteLine("USAGE:codeanalyzer filepath filepattern [/R][/X][/S]");
                return false;
            }
            if (TypeRepository.Instance.RootScope.Child.Count == 0)
            {
                System.Console.WriteLine("Error! The file doesn't exit or the file has no contents!");
                return false;
            }
            TypeRepository.Instance._CompleteUrlForEachMemeber(TypeRepository.Instance.RootScope);
            /*
             * Show types
             *  
             */
            System.Console.WriteLine("");
            System.Console.WriteLine("Type Table");
            System.Console.WriteLine("");
            OutputManager.DisplayTypes(TypeRepository.Instance.RootScope);
            System.Console.WriteLine("");
            /*
             * Show relats
             *  
             */
            if (cmdp.R)
            {
                System.Console.WriteLine("Relationship Table");
                System.Console.WriteLine("");
                TypeRepository.Instance.PruneInvalidComposing();
                TypeRepository.Instance.PruneBoring();
                //TypeRepository.Instance.PruneIrrelevant();
                OutputManager.DisplayRelationships(TypeRepository.Instance.RootScope);
                System.Console.WriteLine("");
            }
            /*
             * Write XML
             *  
             */
            if (cmdp.X)
            {
                if(cmdp.XMLName.Length==0)
                    OutputManager.WriteTypes2XML(TypeRepository.Instance.RootScope, "a.xml");
                else
                    OutputManager.WriteTypes2XML(TypeRepository.Instance.RootScope, cmdp.XMLName);
            }
            return true;
        }
    }
    class TestAna
    { 
#if(TEST_ANALYZER)
         static void Main(string[] args)
        {
            Analyzer t = new Analyzer();
            t.Start(args);
            System.Console.ReadLine();
        }
#endif
    }
}
