namespace Raven.Magic.Client.MagicDocumentSession
{
    using System;
    using System.Collections.Generic;
    using System.Linq.Expressions;
    using Abstractions.Data;
    using Abstractions.Extensions;
    using Abstractions.Indexing;
    using Raven.Client;
    using Raven.Client.Spatial;

    public class QueryIncludesParser : IDocumentQueryCustomization
    {
        public List<string> Includes = new List<string>();

        public IDocumentQueryCustomization Include<TResult, TInclude>(Expression<Func<TResult, object>> path)
        {
            Includes.Add(path.ToPropertyPath());
            return this;
        }

        public IDocumentQueryCustomization Include(string path)
        {
            Includes.Add(path);
            return this;
        }

        #region NotUsed

        public IDocumentQueryCustomization WaitForNonStaleResultsAsOfLastWrite()
        {
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResultsAsOfLastWrite(TimeSpan waitTimeout)
        {
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResultsAsOfNow()
        {
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResultsAsOfNow(TimeSpan waitTimeout)
        {
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResultsAsOf(DateTime cutOff)
        {
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResultsAsOf(DateTime cutOff, TimeSpan waitTimeout)
        {
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResultsAsOf(Etag cutOffEtag)
        {
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResultsAsOf(Etag cutOffEtag, TimeSpan waitTimeout)
        {
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResults()
        {
            return this;
        }

        public IDocumentQueryCustomization Include<TResult>(Expression<Func<TResult, object>> path)
        {
            Includes.Add(path.ToPropertyPath());
            return this;
        }

        public IDocumentQueryCustomization WaitForNonStaleResults(TimeSpan waitTimeout)
        {
            return this;
        }

        public IDocumentQueryCustomization WithinRadiusOf(double radius, double latitude, double longitude)
        {
            return this;
        }

        public IDocumentQueryCustomization WithinRadiusOf(string fieldName, double radius, double latitude, double longitude)
        {
            return this;
        }

        public IDocumentQueryCustomization WithinRadiusOf(double radius, double latitude, double longitude, SpatialUnits radiusUnits)
        {
            return this;
        }

        public IDocumentQueryCustomization WithinRadiusOf(string fieldName, double radius, double latitude, double longitude, SpatialUnits radiusUnits)
        {
            return this;
        }

        public IDocumentQueryCustomization RelatesToShape(string fieldName, string shapeWKT, SpatialRelation rel)
        {
            return this;
        }

        public IDocumentQueryCustomization Spatial(string fieldName, Func<SpatialCriteriaFactory, SpatialCriteria> clause)
        {
            return this;
        }

        public IDocumentQueryCustomization SortByDistance()
        {
            return this;
        }

        public IDocumentQueryCustomization RandomOrdering()
        {
            return this;
        }

        public IDocumentQueryCustomization RandomOrdering(string seed)
        {
            return this;
        }

        public IDocumentQueryCustomization BeforeQueryExecution(Action<IndexQuery> action)
        {
            return this;
        }

        public IDocumentQueryCustomization TransformResults(Func<IndexQuery, IEnumerable<object>, IEnumerable<object>> resultsTransformer)
        {
            return this;
        }

        public IDocumentQueryCustomization Highlight(string fieldName, int fragmentLength, int fragmentCount, string fragmentsField)
        {
            return this;
        }

        public IDocumentQueryCustomization Highlight(string fieldName, int fragmentLength, int fragmentCount, out FieldHighlightings highlightings)
        {
            highlightings = new FieldHighlightings("");
            return this;
        }

        public IDocumentQueryCustomization SetHighlighterTags(string preTag, string postTag)
        {
            return this;
        }

        public IDocumentQueryCustomization SetHighlighterTags(string[] preTags, string[] postTags)
        {
            return this;
        }

        public IDocumentQueryCustomization NoTracking()
        {
            return this;
        }

        public IDocumentQueryCustomization NoCaching()
        {
            return this;
        }

        #endregion
    }
}