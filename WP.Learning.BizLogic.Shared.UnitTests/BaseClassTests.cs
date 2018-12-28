using System;
using System.Collections.Generic;
using System.Text;

using Xunit;
using XUnitPriorityOrderer;

// https://github.com/frederic-prusse/XUnitPriorityOrderer

// set to be sequential execution
[assembly: CollectionBehavior(DisableTestParallelization = true)]
// set the custom test's collection orderer
[assembly: TestCollectionOrderer(CollectionPriorityOrderer.TypeName, CollectionPriorityOrderer.AssembyName)]
namespace WP.Learning.BizLogic.Shared.UnitTests
{
    // set the custom test's case orderer
    [TestCaseOrderer(CasePriorityOrderer.TypeName, CasePriorityOrderer.AssembyName)]
    public abstract class BaseClassTests { }
}
