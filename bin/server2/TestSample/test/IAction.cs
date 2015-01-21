/*
 * Manual Page:
 * 
 * BaseAction //base class for other action
 * class MyAction : BaseAction
 * {
 *      public override void Do(Object args)
        {
            //YOUR CODE HERE!
        }
 * }
 * 
 * Maintenance History:
 * Ver 1.0  Oct. 4  2014 created by Shikai Jin 
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeAnalyzer
{
    /*
     * Action will be executed when rule returns true
     *  
     */
    public class BaseAction
    {
        public virtual void Do(Object args)
        {
            //string str = (string)args;
        }
    }
}
