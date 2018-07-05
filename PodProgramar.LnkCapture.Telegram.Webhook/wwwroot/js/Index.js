var $$ = Dom7;
var pageSize = 10;
var searchTerm;
var user;
var startDate;
var endDate;
var preloader;

var app = new Framework7({
    root: '#app',
    name: 'LnkCapture',
    id: 'podprogramar.lnkcapture',
    theme: 'md',
    panel: {
        swipe: 'left',
    },
    on: {
        pageInit: function (page) {
            startDate = $$('#DefaultStartDate').val();
            endDate = $$('#DefaultEndDate').val();

            if (page.name == "home")
                search(0);
        }
    },
    routes: [
        {
            path: '/',
            name: "home",
            pageInit: function (event, page) {
            }
        },
        {
            path: '/filters/',
            content: '\
                      <div class="page">\
                        <div class="navbar">\
                          <div class="navbar-inner sliding">\
                            <div class="left">\
                              <a href="#" class="link back">\
                                <i class="icon icon-back"></i>\
                                <span class="ios-only">Back</span>\
                              </a>\
                            </div>\
                            <div class="title">Filters</div>\
                          </div>\
                        </div>\
                        <div class="fab fab-right-bottom">\
                            <a href="#" id="applyFilters" class="">\
                                <i class="icon material-icons md-only">done</i>\
                            </a>\
                        </div>\
                        <div class="page-content">\
                          <div class="block">\
                            <div class="list no-hairlines-md">\
                              <ul>\
                                <li class="item-content item-input">\
                                  <div class="item-inner">\
                                    <div class="item-title item-label">Search</div>\
                                    <div class="item-input-wrap">\
                                      <input type="text" id="search" name="search">\
                                  </div>\
                                </li>\
                                <li class="item-content item-input">\
                                  <div class="item-inner">\
                                    <div class="item-title item-label">User</div>\
                                    <div class="item-input-wrap">\
                                      <input type="text" id="user" name="user">\
                                    </div>\
                                  </div>\
                                </li>\
                                <li class="item-content item-input item-input-with-value">\
                                  <div class="item-inner">\
                                  <div class="item-title item-label">Start Date</div>\
                                    <div class="item-input-wrap">\
                                      <input id="startDate" name="startDate" type="date" value="' + $$('#DefaultStartDate').val() + '" placeholder="Please choose..." class="input-with-value">\
                                    </div>\
                                  </div>\
                                </li>\
                                <li class="item-content item-input item-input-with-value">\
                                  <div class="item-inner">\
                                  <div class="item-title item-label">End Date</div>\
                                    <div class="item-input-wrap">\
                                      <input id="endDate" name="endDate" type="date" value="' + $$('#DefaultEndDate').val() + '" placeholder="Please choose..." class="input-with-value">\
                                    </div>\
                                  </div>\
                                </li>\
                              </ul >\
                            </div>\
                        </div>\
                      </div>\
                    ',
            on: {
                pageBeforeIn: function (event, page) {
                    $$('#search').val(searchTerm);
                    $$('#user').val(user);
                    $$('#startDate').val(startDate);
                    $$('#endDate').val(endDate);

                    $$('#applyFilters').on('click', function () {
                        searchTerm = $$('#search').val();
                        user = $$('#user').val();
                        startDate = $$('#startDate').val();
                        endDate = $$('#endDate').val();
                        pageIndex = 0;
                        pageSize = pageSize;

                        search(0);
                        app.router.back();
                    });
                },
                pageAfterIn: function (event, page) {
                },
                pageInit: function (event, page) {
                },
                pageBeforeRemove: function (event, page) {
                }
            }
        }
    ]
});

var mainView = app.views.create('.view-main', {
    main: true,
});

function search(pageIndex) {
    preloader = app.dialog.preloader();

    var data = {
        id: encodeURIComponent($$('#ChatIdEncrypted').val()),
        search: searchTerm,
        user: user,
        startDate: startDate,
        endDate: endDate,
        pageIndex: pageIndex,
        pageSize: pageSize
    }

    app.request({
        url: "/api/link",
        dataType: 'json',
        type: 'get',
        data: data,
        crossDomain: true,
        headers: {
            'Accept': 'application/json; odata=verbose', 'Content-Type': 'application/json'
        },
        statusCode: {
            404: function (xhr) {
                console.log('page not found');
            }
        },
        complete: function (data) {
            preloader.close();
        },
        success: function (response) {
            $$('#tblinks tbody').empty();
            $$('.data-table-footer').empty();

            for (var i = 0; i < response.items.length; i++) {
                var tr = $$('<tr>')
                var tdTitle = $$('<td style="white-space: nowrap">');
                var aTitle = $$('<a class="link external" target="_blank" href=' + response.items[i].uri + '>');
                var tdUser = $$('<td>');
                var tdCreate = $$('<td>');
                var createDate = new Date(response.items[i].createDate);

                if (response.items[i].title != undefined) {
                    if (response.items[i].title.length < 55)
                        aTitle.text(response.items[i].title);
                    else
                        aTitle.text(response.items[i].title.substring(0, 52) + '...');
                } else {
                    if (response.items[i].uri.length < 55)
                        aTitle.text(response.items[i].uri);
                    else
                        aTitle.text(response.items[i].uri.substring(0, 52) + '...');
                }

                if (response.items[i].username != undefined) {
                    tdUser.text(response.items[i].username);
                } else if (response.items[i].userId != undefined)
                    tdUser.text(response.items[i].userId);
                else
                    tdUser.text('-');

                tdCreate.text(formatDate(createDate));

                tdTitle.append(aTitle);
                tr.append(tdTitle);
                tr.append(tdUser);
                tr.append(tdCreate);

                $$('#tblinks tbody').append(tr);
            }

            var lastPage = Math.ceil(response.totalSearchItems / pageSize);
            var paginationSelect = $$('<div class="data-table-rows-select">\
                                            Per page:\
                                            <div class="input input-dropdown">\
                                                <select>\
                                                    <option value="10" ' + (pageSize == 10 ? 'selected' : '') + '>10</option>\
                                                    <option value="25" ' + (pageSize == 25 ? 'selected' : '') + '>25</option>\
                                                    <option value="50" ' + (pageSize == 50 ? 'selected' : '') + '>50</option>\
                                                    <option value="100" ' + (pageSize == 100 ? 'selected' : '') + '>100</option>\
                                                </select>\
                                            </div>\
                                        </div>');

            $$(paginationSelect).find('select').on('change', function () {
                pageSize = $$('.data-table-rows-select select').val();
                search(0);
            });

            var pagination = $$('<div class="data-table-pagination"></div>');
            var paginationLabel = $$('<span class="data-table-pagination-label"> ' + ((pageIndex * pageSize) + 1) + ' - ' + (((pageIndex * pageSize)) + response.items.length) + ' of ' + response.totalSearchItems + '</span>');
            var paginationPrevious = $$('<a href="#" class="link ' + (pageIndex == 0 ? 'disabled' : '') + '">\
                                            <i class="icon icon-prev color-gray"></i>\
                                        </a>');

            var paginationNext = $$('<a href="#" class="link ' + (pageIndex + 1 == lastPage ? 'disabled' : '') + '">\
                                        <i class="icon icon-next color-gray"></i>\
                                    </a>');

            paginationPrevious.on('click', function (e) {
                search(pageIndex - 1);
            });

            paginationNext.on('click', function (e) {
                search(pageIndex + 1);
            });

            pagination.append(paginationLabel);
            pagination.append(paginationPrevious);
            pagination.append(paginationNext);

            $$('.data-table-footer').append(paginationSelect);
            $$('.data-table-footer').append(pagination);
        },
        error: function (xhr, status) {
            console.log(status);
        }
    });
}

function formatDate(date) {
    return (date.getDate() <= 9 ? '0' : '') + date.getDate() + '/'
        + (date.getMonth() + 1 <= 9 ? '0' : '') + (date.getMonth() + 1) + '/'
        + date.getFullYear() + ' '
        + (date.getHours() <= 9 ? '0' : '') + date.getHours() + ':'
        + (date.getMinutes() <= 9 ? '0' : '') + date.getMinutes();
}