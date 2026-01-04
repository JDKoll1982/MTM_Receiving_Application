using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Module_Core.Contracts.Services
{
    public interface IService_Pagination
    {
        /// <summary>
        /// Gets the current page number (1-based).
        /// </summary>
        public int CurrentPage { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        public int TotalPages { get; }

        /// <summary>
        /// Gets the number of items per page.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets the total number of items in the source collection.
        /// </summary>
        public int TotalItems { get; }

        /// <summary>
        /// Gets a value indicating whether there is a next page.
        /// </summary>
        public bool HasNextPage { get; }

        /// <summary>
        /// Gets a value indicating whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage { get; }

        /// <summary>
        /// Event raised when the current page data changes.
        /// </summary>
        public event EventHandler PageChanged;

        /// <summary>
        /// Sets the source collection to be paginated.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="source">The source collection.</param>
        public void SetSource<T>(IEnumerable<T> source);

        /// <summary>
        /// Gets the items for the current page.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <returns>A list of items for the current page.</returns>
        public IEnumerable<T> GetCurrentPageItems<T>();

        /// <summary>
        /// Moves to the next page.
        /// </summary>
        public bool NextPage();

        /// <summary>
        /// Moves to the previous page.
        /// </summary>
        public bool PreviousPage();

        /// <summary>
        /// Moves to the first page.
        /// </summary>
        public bool FirstPage();

        /// <summary>
        /// Moves to the last page.
        /// </summary>
        public bool LastPage();

        /// <summary>
        /// Moves to a specific page.
        /// </summary>
        /// <param name="pageNumber">The page number to move to.</param>
        public bool GoToPage(int pageNumber);
    }
}

