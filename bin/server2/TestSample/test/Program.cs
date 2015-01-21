/*
 * Manual Page:
 * 
 * Entry of Whole project
 * turn on the macro PROGRAM, before running
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 3  2014 created by Shikai Jin 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /*
     * The codeanalyzer's main entry
     *   
     */

    class Program
    {

#if(PROGRAM)
        static void Main(string[] args)
        {
            Analyzer a = new Analyzer();
            a.Start(args);


        
        }
#endif
    }
}
