/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 
  Introduction:
 If a semi passes the parent rule, it will continue to be tested under each child rule. 
Each rule has actions which will be automatically invoked by the parser when the rule testing returns true. 
For example, the rules for detecting class, functions, etc. are all children of rules of detecting scopes.
So if a semi is tested to be the beginning of a scope, parser will continue to see if it is class scope or function scope.
The rules of detecting declaration statement, break statement are all children of detecting general statement.
So if a semi is tested to be a statement, parser will continue to determine what statement the semi is.
if a semi does not pass the rule, parser will move to its another sibling rule without going down to its child rules.

 */
/*
 * Manual Page:
 * 
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
/*
 *   Build command:
 *   devenv ./DependencyAnalyzer.sln /rebuild debug
 *   
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
        private  bool _IsSubruleExclusive = false;
        public bool IsSubruleExclusive 
        { 
            set { _IsSubruleExclusive = value; }
            get { return _IsSubruleExclusive; }
        }
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

//test stub
// interface tested with other packages