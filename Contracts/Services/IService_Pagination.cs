using System;
using System.Collections.Generic;

namespace MTM_Receiving_Application.Contracts.Services
{
    public interface IService_Pagination
    {
        /// <summary>
        /// Gets the current page number (1-based).
        /// </summary>
        int CurrentPage { get; }

        /// <summary>
        /// Gets the total number of pages.
        /// </summary>
        int TotalPages { get; }

        /// <summary>
        /// Gets the number of items per page.
        /// </summary>
        int PageSize { get; set; }

        /// <summary>
        /// Gets the total number of items in the source collection.
        /// </summary>
        int TotalItems { get; }

        /// <summary>
        /// Gets a value indicating whether there is a next page.
        /// </summary>
        bool HasNextPage { get; }

        /// <summary>
        /// Gets a value indicating whether there is a previous page.
        /// </summary>
        bool HasPreviousPage { get; }

        /// <summary>
        /// Event raised when the current page data changes.
        /// </summary>
        event EventHandler PageChanged;

        /// <summary>
        /// Sets the source collection to be paginated.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <param name="source">The source collection.</param>
        void SetSource<T>(IEnumerable<T> source);

        /// <summary>
        /// Gets the items for the current page.
        /// </summary>
        /// <typeparam name="T">The type of items in the collection.</typeparam>
        /// <returns>A list of items for the current page.</returns>
        IEnumerable<T> GetCurrentPageItems<T>();

        /// <summary>
        /// Moves to the next page.
        /// </summary>
        void NextPage();

        /// <summary>
        /// Moves to the previous page.
        /// </summary>
        void PreviousPage();

        /// <summary>
        /// Moves to the first page.
        /// </summary>
        void FirstPage();

        /// <summary>
        /// Moves to the last page.
        /// </summary>
        void LastPage();

        /// <summary>
        /// Moves to a specific page.
        /// </summary>
        /// <param name="pageNumber">The page number to move to.</param>
        void GoToPage(int pageNumber);
    }
}
