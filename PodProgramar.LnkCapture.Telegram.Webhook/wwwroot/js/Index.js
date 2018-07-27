var $$ = Dom7;
var pageSize = 10;
var searchTerm;
var user;
var startDate;
var endDate;
var lastChatId;

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
        },
        pageAfterIn: function (page) {
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
                            <div class="title">Pesquisar</div>\
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
                                    <div class="item-title item-label">Pesquisa</div>\
                                    <div class="item-input-wrap">\
                                      <input type="text" id="search" name="search">\
                                  </div>\
                                </li>\
                                <li class="item-content item-input">\
                                  <div class="item-inner">\
                                    <div class="item-title item-label">Usuário</div>\
                                    <div class="item-input-wrap">\
                                      <input type="text" id="user" name="user">\
                                    </div>\
                                  </div>\
                                </li>\
                                <li class="item-content item-input item-input-with-value">\
                                  <div class="item-inner">\
                                  <div class="item-title item-label">Data inicial</div>\
                                    <div class="item-input-wrap">\
                                      <input id="startDate" name="startDate" type="date" value="' + $$('#DefaultStartDate').val() + '" placeholder="Please choose..." class="input-with-value">\
                                    </div>\
                                  </div>\
                                </li>\
                                <li class="item-content item-input item-input-with-value">\
                                  <div class="item-inner">\
                                  <div class="item-title item-label">Data final</div>\
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
    main: true
});

app.smartSelect.get(document.getElementById('chatId').element).on('closed', function () {
    if ($$('#chatId')[0].selectedOptions[0].value != lastChatId) {
        search(0);
    }
});

app.smartSelect.get(document.getElementById('chatId').element).params.sheetCloseLinkText = "APLICAR";

function search(pageIndex) {
    $$('.page-content').scrollTop(0, 500);

    app.preloader.show('orange');

    lastChatId = $$('#chatId')[0].selectedOptions[0].value;

    var data = {
        search: searchTerm,
        user: user,
        startDate: startDate,
        endDate: endDate,
        pageIndex: pageIndex,
        pageSize: pageSize
    };

    app.request({
        url: "/api/link/" + $$('#chatId')[0].selectedOptions[0].value,
        dataType: 'json',
        type: 'get',
        data: data,
        crossDomain: true,
        headers: {
            'Accept': 'application/json; odata=verbose', 'Content-Type': 'application/json',
            'IsAPIRequest': false
        },
        statusCode: {
            404: function (xhr) {
                console.log('page not found');
            }
        },
        complete: function (data) {
            app.preloader.hide();
        },
        success: function (response) {
            $$('#cards').empty();
            $$('.data-table-footer').empty();

            for (var i = 0; i < response.items.length; i++) {
                var monthNames = ["Jan", "Fev", "Mar", "Abr", "Mai", "Jun", "Jul", "Ago", "Set", "Out", "Nov", "Dez"];

                var thumbnailUri = response.items[i].thumbnailUri ? response.items[i].thumbnailUri : '/content/images/nothumbnail.jpg';
                var title = response.items[i].title ? (response.items[i].title.length < 140 ? response.items[i].title : response.items[i].title.substring(0, 137) + '...') : (response.items[i].uri.length < 250 ? response.items[i].uri : response.items[i].uri.substring(0, 247) + '...');
                var username = response.items[i].firstName ? response.items[i].firstName + (response.items[i].lastName ? ' ' + response.items[i].lastName : '') : (response.items[i].username ? response.items[i].username : response.items[i].userId);
                var createDate = new Date(response.items[i].createDate);
                var createDateFormatted = createDate.getDate() + ' de ' + monthNames[createDate.getMonth()] + ' de ' + createDate.getFullYear() + ' por ' + username;
                var description = response.items[i].description != null ? (response.items[i].description.length < 200 ? response.items[i].description : response.items[i].description.substring(0, 197) + '...') : null;

                var cardElement = $$('<div class="card demo-card-header-pic">');
                var headerElement = $$('<div valign="bottom" class="card-header"><div class="card-background" style="background: url(/content/images/nothumbnail.jpg)"></div>' + (response.items[i].thumbnailUri ? '<div class="card-background" style="background-color: #ffffff !important; background: url(\'' + thumbnailUri + '\')"></div>' : '') + '<div class="card-background card-background-transparency"></div>' + title + '</div>');
                var cardContentElement = $$('<div class="card-content card-content-padding">');
                var dateElement = $$('<p class="date">' + createDateFormatted + '</p>');
                var descriptionElement = $$('<p class="">' + description + '</p>');
                var cardFooterElement = $$('<div class="card-footer"><a href="' + response.items[i].uri + '" class="link external" target="_blank">Abrir</a>'); //<a href="#" class="link">Read more</a></div>

                cardElement.append(headerElement);
                cardElement.append(cardContentElement);
                cardContentElement.append(dateElement);

                if (description != null)
                    cardContentElement.append(descriptionElement);

                cardElement.append(cardFooterElement);

                $$('#cards').append(cardElement);
            }

            var lastPage = Math.ceil(response.totalSearchItems / pageSize);
            var paginationSelect = $$('<div class="data-table-rows-select">\
                                            Itens:\
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
            var paginationLabel = $$('<span class="data-table-pagination-label"> ' + ((pageIndex * pageSize) + 1) + ' - ' + (((pageIndex * pageSize)) + response.items.length) + ' de ' + response.totalSearchItems + '</span>');
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