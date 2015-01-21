/*
 * Manual Page:
 * 
 * Scope s = RuleMaker.Make(); //Get the rule that is able to satisfied the requirement
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
     *Organize rules in tree structure.
     * passes parent node, it will go down to child node.
     * 
     * if a semi proves to be a statement, it will continue to try 
     * the subrules to see of it is a new statement.
     */
    public class RuleMaker
    {
        public static BaseRule Make()
        {
            TrueRule tr = new TrueRule();
            //detect statement
            IsStatement isstate = new IsStatement();
            {
                // if a semi passes statement detection, it will try the followings

                //detect member variable
                IsMemberDeclaration ismember = new IsMemberDeclaration();
                ismember.AddAction(new OnAddScopes());
                isstate.AddSubrule(ismember);
                //detect new statement
                IsNewStatement isnewstatement = new IsNewStatement();
                isstate.AddSubrule(isnewstatement);
                //detect new property
                IsPropertyMember ispropertymember = new IsPropertyMember();
                isstate.AddSubrule(ispropertymember);
                //detect delegate
                IsDelegate isdelegate = new IsDelegate();
                isdelegate.AddAction(new OnAddScope());
                isstate.AddSubrule(isdelegate);

                //detect break
                IsBreak isbreak = new IsBreak();
                isstate.AddSubrule(isbreak);
                {
                    // detect break inside a loop
                    IsLoopBreak isloopbreak = new IsLoopBreak();
                    isloopbreak.AddAction(new OnAddScope());
                    isbreak.AddSubrule(isloopbreak);

                }
            }

            tr.AddSubrule(isstate);

            //detect enter scope
            IsEnterScope isenter = new IsEnterScope();
            isenter.IsSubruleExclusive = true;
            tr.AddSubrule(isenter);

            {
                //namespace
                IsNamespace isname = new IsNamespace();
                isname.AddAction(new OnAddAndEnterScope());
                isenter.AddSubrule(isname);

                //detect class
                IsClass isclass = new IsClass();
                isclass.AddAction(new OnAddAndEnterScope());
                {
                    IsInheritance isinhe = new IsInheritance();
                    isinhe.AddAction(new OnAddScope());
                    isclass.AddSubrule(isinhe);

                }
                isenter.AddSubrule(isclass);

                //detect property
                IsProperty isproperty = new IsProperty();
                isproperty.AddAction(new OnAddAndEnterScope());
                isenter.AddSubrule(isproperty);
                //detect function
                IsFunction isfunction = new IsFunction();
                isfunction.AddAction(new OnAddAndEnterScope());
                {
                    IsUsingFunction isusing = new IsUsingFunction();
                    isfunction.AddSubrule(isusing);
                }
                isenter.AddSubrule(isfunction);
                //detect all other scopes
                IsOtherScope isotherscope = new IsOtherScope();
                isotherscope.AddAction(new OnAddAndEnterScope());
                isenter.AddSubrule(isotherscope);
            }
            //detect leaving scope
            IsLeavingScope isleaving = new IsLeavingScope();
            isleaving.AddAction(new OnLeanvingScope());
            tr.AddSubrule(isleaving);
            return tr;
        }
    }



#if(TEST_RULEMAKER)
    class TestScope
    {
        static void Main(string[] args)
        {
            RuleTester rt = new RuleTester();
            rt.AddRule(RuleMaker.Make());
            SemiExtractor se = new SemiExtractor();
            se.Open("../../Semi.cs");
            rt.Test(se);
            OutputManager.DisplayTypes(TypeRepository.Instance.CurrentScope);
            System.Console.ReadLine();
        }
    }
#endif
}
