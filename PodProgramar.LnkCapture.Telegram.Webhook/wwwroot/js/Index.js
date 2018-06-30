var pageSize = 10;

$(document).ready(function () {
    $('#btnFilter').on('click', function () {
        search(0);
    });

    search(0);
});

function search(pageIndex) {
    var data = {
        search: $('#search').val(),
        user: $('#user').val(),
        startDate: $('#startDate').val(),
        endDate: $('#endDate').val(),
        pageIndex: pageIndex,
        pageSize: pageSize
    }

    $.ajax({
        type: "GET",
        contentType: "application/json",
        url: "/api/Link/" + encodeURIComponent($('#ChatIdEncrypted').val()),
        data: data,
        dataType: "json"
    }).done(function (data) {
        $('#tblinks tbody').empty();
        $('.pagination').empty();

        $('#chatTitle').text(data.chatTitle);
        $('#totalItems').text(data.totalItems);
        $('#totalSearchItems').text(data.totalSearchItems);

        for (var i = 0; i < data.items.length; i++) {
            var tr = $('<tr/>')
            var tdTitle = $('<td/>', { css: { 'white-space': 'nowrap' } });
            var aTitle = $('<a/>', { target: '_blank', href: data.items[i].uri });
            var tdUser = $('<td/>');
            var tdCreate = $('<td/>');
            var createDate = new Date(data.items[i].createDate);

            if (data.items[i].title != undefined) {
                if (data.items[i].title.length < 75)
                    aTitle.text(data.items[i].title);
                else
                    aTitle.text(data.items[i].title.substring(0, 73) + '...');
            } else {
                if (data.items[i].uri.length < 75)
                    aTitle.text(data.items[i].uri);
                else
                    aTitle.text(data.items[i].uri.substring(0, 73) + '...');
            }

            if (data.items[i].username != undefined) {
                tdUser.text(data.items[i].username);
            } else if (data.items[i].userId != undefined)
                tdUser.text(data.items[i].userId);
            else
                tdUser.text('-');

            tdCreate.text(formatDate(createDate));

            tdTitle.append(aTitle);
            tr.append(tdTitle);
            tr.append(tdUser);
            tr.append(tdCreate);

            $('#tblinks tbody').append(tr);
        }

        var totalPaginationButtons = 6;
        var startPagination = 1;
        var endPagination = Math.ceil(data.totalSearchItems / pageSize) + 1;

        if (endPagination > totalPaginationButtons) {
            if (pageIndex + 1 >= totalPaginationButtons) {
                startPagination = pageIndex - 3;
                endPagination = pageIndex + 3;
            }
            else
                endPagination = totalPaginationButtons + 1;
        }

        for (var i = startPagination; i < endPagination; i++) {
            var li = $('<li/>', { class: 'page-item' });
            var a = $('<a/>', { class: 'page-link', href: '#', text: i });

            if (i == pageIndex + 1)
                li.addClass('active');

            a.on('click', { pageIndex: i - 1 }, function (e) {
                search(e.data.pageIndex);
            })

            li.append(a);

            $('.pagination').append(li);
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