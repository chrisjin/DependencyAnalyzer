/*
 * Manual Page:
 * 
 * BaseRule //base rule for other rules
 * class MyRule:BaseRule
 * {
 *  public override bool Test(SemiExpression semi)
 *  {
 *      //YOUR CODE HERE! Either returns true or false!
 *  }
 * }
 * 
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
     * Base class for all rules
     * A rule can have multiple child rules.
     * the behavior of child rules are controlled by thier parents
     */
    public class BaseRule
    {
        /*
         * if test returns true,its child rules will be tested 
         *  and the related actions will execute.
         */        
        public virtual bool Test(SemiExpression semi)
        {
            return true;
        }
        /*
         * Add child rules
         *  
         */
        public void AddSubrule(BaseRule r)
        {
            ChildRule.Add(r);
        }
        /*
         * add related actions
         *  
         */
        public void AddAction(BaseAction a)
        {
            Action.Add(a);
        }
        /*
         * getter of childrule
         *  
         */
        public List<BaseRule> ChildRule
        {
            get 
            {
                return _ChildRule;
            }
        }
        /*
         * getter of actions
         *  
         */
        public List<BaseAction> Action
        {
            get 
            {
                return _Action;
            }
        }
        private List<BaseRule> _ChildRule=new List<BaseRule>();
        private List<BaseAction> _Action = new List<BaseAction>();
        public  Object Arg { set; get; }
        public  bool IsSubruleExclusive = false;
    }
    public class TrueRule : BaseRule
    {
        /*
         * a rule that always is true.
         * that can be used to serve as the root  of a rule tree.
         *  
         */
        public override bool Test(SemiExpression semi)
        {
            return true;
        }
    }


}
