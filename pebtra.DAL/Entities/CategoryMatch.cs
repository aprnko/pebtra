using System;

namespace pebtra.DAL
{
    public partial class CategoryMatch
    {
        public int Id { get; set; }
        public required string CategoryId { get; set; }
        public required string MatchText { get; set; }
    }
} 