/*
 *   Shikai Jin 
 *   sjin02@syr.edu
 *   SUID 844973756
 */
/*
 * Manual Page:
 * 
 * Scope s = RuleMaker.Make(); //Get the rule that is able to satisfied the requirement
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
     *Organize rules in tree structure.
     * passes parent node, it will go down to child node.
     * 
     * if a semi proves to be a statement, it will continue to try 
     * the subrules to see of it is a new statement.
     */
    public class RuleMaker
    {
        static void AddStatementSubrule(IsStatement isstate)
        {
            IsMemberDeclaration ismember = new IsMemberDeclaration();
            ismember.AddAction(new OnAddScopes());
            isstate.AddSubrule(ismember);

            IsNewStatement isnewstatement = new IsNewStatement();
            isnewstatement.AddAction(new OnAddScope());
            isstate.AddSubrule(isnewstatement);
            
            IsPropertyMember ispropertymember = new IsPropertyMember();
            isstate.AddSubrule(ispropertymember);
            
            IsDelegate isdelegate = new IsDelegate();
            isdelegate.AddAction(new OnAddScope());
            isstate.AddSubrule(isdelegate);
            
            IsBreak isbreak = new IsBreak();
            isstate.AddSubrule(isbreak);
            {
                IsLoopBreak isloopbreak = new IsLoopBreak();
                isloopbreak.AddAction(new OnAddScope());
                isbreak.AddSubrule(isloopbreak);
            }
        }
        static void AddEnterScopeSubrule(IsEnterScope isenter)
        {
            IsNamespace isname = new IsNamespace();
            isname.AddAction(new OnAddAndEnterScope());
            isenter.AddSubrule(isname);
            IsClass isclass = new IsClass();
            isclass.AddAction(new OnAddAndEnterScope());
            {
                IsInheritance isinhe = new IsInheritance();
                isinhe.AddAction(new OnAddScope());
                isclass.AddSubrule(isinhe);

            }
            isenter.AddSubrule(isclass);
            IsProperty isproperty = new IsProperty();
            isproperty.AddAction(new OnAddAndEnterScope());
            isenter.AddSubrule(isproperty);
            IsFunction isfunction = new IsFunction();
            isfunction.AddAction(new OnAddAndEnterScope());
            {
                IsUsingFunction isusing = new IsUsingFunction();
                isfunction.AddSubrule(isusing);
            }
            isenter.AddSubrule(isfunction);
            IsOtherScope isotherscope = new IsOtherScope();
            isotherscope.AddAction(new OnAddAndEnterScope());
            isenter.AddSubrule(isotherscope);
        }
        public static BaseRule Make()
        {
            TrueRule tr = new TrueRule();
            IsStatement isstate = new IsStatement();
            AddStatementSubrule(isstate);
            tr.AddSubrule(isstate);

            IsEnterScope isenter = new IsEnterScope();
            isenter.IsSubruleExclusive = true;
            AddEnterScopeSubrule(isenter);
            tr.AddSubrule(isenter);

            IsLeavingScope isleaving = new IsLeavingScope();
            isleaving.AddAction(new OnLeanvingScope());
            tr.AddSubrule(isleaving);
            return tr;
        }
    }


    //Test Stub
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
