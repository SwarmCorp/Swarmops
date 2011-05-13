using System;
using System.Collections.Generic;
using Activizr.Logic.Pirates;
using Activizr.Logic.Structure;
using Activizr.Logic.Support;
using Activizr.Basic.Enums;
using Activizr.Basic.Types;
using Activizr.Database;

namespace Activizr.Logic.Financial
{
    public class FinancialAccounts : PluralBase<FinancialAccounts,FinancialAccount,BasicFinancialAccount>
    {
        public static FinancialAccounts ForOrganization(Organization organization)
        {
            return FromArray(PirateDb.GetDatabase().GetFinancialAccountTreeForOrganization(organization.Identity));
        }

        public static FinancialAccounts ForOwner(Person person)
        {
            return FromArray(PirateDb.GetDatabase().GetFinancialAccountsOwnedByPerson(person.Identity));
        }



        public static FinancialAccounts ForOrganization (Organization organization, FinancialAccountType accountType)
        {
            FinancialAccounts allAccounts = ForOrganization(organization);
            FinancialAccounts result = new FinancialAccounts();

            Dictionary<FinancialAccountType, bool> lookup = new Dictionary<FinancialAccountType, bool>();
            lookup[accountType] = true;

            if (accountType == FinancialAccountType.Balance || accountType == FinancialAccountType.Unknown)
            {
                lookup[FinancialAccountType.Asset] = true;
                lookup[FinancialAccountType.Debt] = true;
            }

            if (accountType == FinancialAccountType.Result || accountType == FinancialAccountType.Unknown)
            {
                lookup[FinancialAccountType.Income] = true;
                lookup[FinancialAccountType.Cost] = true;
            }

            foreach (FinancialAccount account in allAccounts)
            {
                if (lookup.ContainsKey(account.AccountType))
                {
                    result.Add(account);
                }
            }

            return result;
        }

        public static FinancialAccounts FromBankTransactionTag (string tag)
        {
            int[] accountIdentities = PirateDb.GetDatabase().GetObjectsByOptionalData(ObjectType.FinancialAccount,
                                                                                      ObjectOptionalDataType.
                                                                                          BankTransactionTag,
                                                                                      tag.ToLower());

            return FromIdentities(accountIdentities);
        }

        public static FinancialAccounts FromIdentities(int[] financialAccountIds)
        {
            if (financialAccountIds.Length == 0)
            {
                return new FinancialAccounts();
            }

            return FromArray(PirateDb.GetDatabase().GetFinancialAccounts(financialAccountIds));
        }

        public FinancialAccountRows GetRows (DateTime start, DateTime end)
        {
            BasicFinancialAccountRow[] basicRows = PirateDb.GetDatabase().GetFinancialAccountRows(Identities, start, end);
            return FinancialAccountRows.FromArray(basicRows);
        }


        public double GetBudgetSum (int year)
        {
            return PirateDb.GetDatabase().GetFinancialAccountsBudget(this.Identities, year);
        }


        public Int64 GetDeltaCents (DateTime start, DateTime end)
        {
            return PirateDb.GetDatabase().GetFinancialAccountBalanceDeltaCents(this.Identities, start, end);
        }


        public static FinancialAccounts GetTree(FinancialAccount rootAccount)
        {
            Dictionary<int, FinancialAccounts> nodes = GetHashedAccounts();

            return GetTree(nodes, rootAccount.Identity, 0);
        }

        
        /*
        public Dictionary<int, BasicGeography> GetGeographyHashtable(int startGeographyId)
        {
            BasicGeography[] nodes = GetTree(startGeographyId);

            Dictionary<int, BasicGeography> result = new Dictionary<int, BasicGeography>();

            foreach (BasicGeography node in nodes)
            {
                result[node.GeographyId] = node;
            }

            return result;
        }*/


        private static FinancialAccounts GetTree(Dictionary<int, FinancialAccounts> accounts, int startNodeId,
                                                   int generation)
        {
            FinancialAccounts result = new FinancialAccounts();

            FinancialAccounts thisList = accounts[startNodeId];

            foreach (FinancialAccount account in thisList)
            {
                if (account.Identity != startNodeId)
                {
                    result.Add(account);

                    // Add recursively

                    FinancialAccounts children = GetTree(accounts, account.Identity, generation + 1);

                    if (children.Count > 0)
                    {
                        foreach (FinancialAccount child in children)
                        {
                            result.Add(child);
                        }
                    }
                }
                else if (generation == 0)
                {
                    // The top parent is special and should be added; the others shouldn't

                    result.Add(account);
                }
            }

            return result;
        }

        protected static Dictionary<int, FinancialAccounts> GetHashedAccounts()
        {
            // This generates a Dictionary <int,List<FinancialAccount>>.
            // 
            // Keys are integers corresponding to NodeIds. At each key n,
            // the value is an List<Node> starting with the node n followed by
            // its children.
            //
            // (Later reflection:) O(n) complexity, instead of recursion. Nice!

            Dictionary<int, FinancialAccounts> result = new Dictionary<int, FinancialAccounts>();

            FinancialAccounts allAccounts = FromArray(PirateDb.GetDatabase().GetFinancialAccountsForOrganization(Organization.PPSEid)); // HACK HACK HACK

            // Add the nodes.

            foreach (FinancialAccount account in allAccounts)
            {
                FinancialAccounts accounts = new FinancialAccounts();
                accounts.Add(account);

                result[account.Identity] = accounts;
            }

            // Add the children.

            foreach (FinancialAccount account in allAccounts)
            {
                if (account.ParentIdentity != 0)
                {
                    result[account.ParentIdentity].Add(account);
                }
            }

            return result;
        }


    }
}