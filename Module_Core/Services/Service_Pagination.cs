using MTM_Receiving_Application.Module_Core.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTM_Receiving_Application.Module_Core.Services
{
    public class Service_Pagination : IService_Pagination
    {
        private IEnumerable<object>? _source;

        /// <summary>
        /// Default page size
        /// </summary>
        private int _pageSize = 20;

        public int CurrentPage { get; private set; } = 1;

        public int TotalPages
        {
            get
            {
                if (_source?.Any() != true)
                {
                    return 1;
                }

                return (int)Math.Ceiling((double)TotalItems / PageSize);
            }
        }

        public int PageSize
        {
            get => _pageSize;
            set
            {
                if (_pageSize != value)
                {
                    _pageSize = value;
                    CurrentPage = 1; // Reset to first page on size change
                    OnPageChanged();
                }
            }
        }

        public int TotalItems => _source?.Count() ?? 0;

        public bool HasNextPage => CurrentPage < TotalPages;

        public bool HasPreviousPage => CurrentPage > 1;

        public event EventHandler? PageChanged;

        public void SetSource<T>(IEnumerable<T> source)
        {
            _source = source?.Cast<object>() ?? Enumerable.Empty<object>();
            CurrentPage = 1;
            OnPageChanged();
        }

        public IEnumerable<T> GetCurrentPageItems<T>()
        {
            if (_source == null)
            {
                return Enumerable.Empty<T>();
            }

            return _source
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Cast<T>();
        }

        public bool NextPage()
        {
            if (HasNextPage)
            {
                CurrentPage++;
                OnPageChanged();
                return true;
            }
            return false;
        }

        public bool PreviousPage()
        {
            if (HasPreviousPage)
            {
                CurrentPage--;
                OnPageChanged();
                return true;
            }
            return false;
        }

        public bool FirstPage()
        {
            if (CurrentPage != 1)
            {
                CurrentPage = 1;
                OnPageChanged();
                return true;
            }
            return false;
        }

        public bool LastPage()
        {
            if (CurrentPage != TotalPages)
            {
                CurrentPage = TotalPages;
                OnPageChanged();
                return true;
            }
            return false;
        }

        public bool GoToPage(int pageNumber)
        {
            if (pageNumber < 1)
            {
                pageNumber = 1;
            }

            if (pageNumber > TotalPages)
            {
                pageNumber = TotalPages;
            }

            if (CurrentPage != pageNumber)
            {
                CurrentPage = pageNumber;
                OnPageChanged();
                return true;
            }
            return false;
        }

        private void OnPageChanged()
        {
            PageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}

