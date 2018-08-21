using Microsoft.EntityFrameworkCore;
using Paillave.Etl.Core.StreamNodes;
using Paillave.Etl.Core.Streams;
using Paillave.RxPush.Core;
using System;
using System.Collections.Generic;
using System.Text;
using Paillave.RxPush.Operators;

namespace Paillave.Etl.EntityFrameworkCore.StreamNodes
{
    public class ToEntityFrameworkStreamNode<TIn, TRes> : ToResourceStreamNodeBase<TIn, TRes, ToResourceStreamArgsBase<TRes>>
        where TRes : DbContext
        where TIn : class
    {
        public ToEntityFrameworkStreamNode(IStream<TIn> input, string name, ToResourceStreamArgsBase<TRes> args) : base(input, name, args) { }

        protected override void ProcessValueToOutput(TRes outputResource, TIn value)
        {
            outputResource.Add(value);
            outputResource.SaveChanges();
        }

        //private void SaveElements(TRes outputResource, IEnumerable<TIn> elements)
        //{
        //    foreach (var value in elements)
        //        outputResource.Add(value);
        //    outputResource.SaveChanges();
        //}
    }
}
