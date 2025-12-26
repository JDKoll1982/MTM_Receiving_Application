using MTM_Receiving_Application.Contracts.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTM_Receiving_Application.Services
{
    public class Service_Pagination : IService_Pagination
    {
        private IEnumerable<object>? _source;
        private int _currentPage = 1;
        private int _pageSize = 20; // Default page size

        public int CurrentPage => _currentPage;

        public int TotalPages
        {
            get
            {
                if (_source == null || !_source.Any()) return 1;
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
                    _currentPage = 1; // Reset to first page on size change
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
            _currentPage = 1;
            OnPageChanged();
        }

        public IEnumerable<T> GetCurrentPageItems<T>()
        {
            if (_source == null) return Enumerable.Empty<T>();

            return _source
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize)
                .Cast<T>();
        }

        public void NextPage()
        {
            if (HasNextPage)
            {
                _currentPage++;
                OnPageChanged();
            }
        }

        public void PreviousPage()
        {
            if (HasPreviousPage)
            {
                _currentPage--;
                OnPageChanged();
            }
        }

        public void FirstPage()
        {
            if (_currentPage != 1)
            {
                _currentPage = 1;
                OnPageChanged();
            }
        }

        public void LastPage()
        {
            if (_currentPage != TotalPages)
            {
                _currentPage = TotalPages;
                OnPageChanged();
            }
        }

        public void GoToPage(int pageNumber)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageNumber > TotalPages) pageNumber = TotalPages;

            if (_currentPage != pageNumber)
            {
                _currentPage = pageNumber;
                OnPageChanged();
            }
        }

        private void OnPageChanged()
        {
            PageChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
