; (function (window, undefined) {
    var data;
    $.ajax({
        url: '/Main/GetTree',
        async: false,
        data: {},
        type: 'get',
        beforeSend: function () {
            //3.让提交按钮失效，以实现防止按钮重复点击

        },
        complete: function () {

        },
        success: function (msg) {
            data = $.parseJSON(msg);
        },
        error: function () {
            alert("系统出现问题");
            return false;
        }
    });

    var treeView = tools.$('#treeView');
    var fileData = data;
    treeView.innerHTML = treeHtml(fileData, 0);
    var fileItem = tools.$('.treeNode');
    var root_icon = tools.$('.icon-control', fileItem[0])[0];
    root_icon.className = 'icon icon-control icon-add';
    tools.each(fileItem, function (item) { filesHandle(item); });
    function treeHtml(fileData, fileId) {
        var _html = '';
        var children = getChildById(fileData, fileId);
        var hideChild = fileId > 0 ? 'none' : '';
        _html += '<ul class="' + hideChild + '">';
        children.forEach(function (item, index) {
            var level = getLevelById(fileData, item.id);
            var distance = (level - 1) * 20 + 'px';
            var hasChild = hasChilds(fileData, item.id);
            var className = hasChild ? '' : 'treeNode-empty';
            var treeRoot_cls = fileId === 0 ? 'treeNode-cur' : '';
            _html += `<li onclick="get(${item.id})"><div class="treeNode ${className} ${treeRoot_cls}" style="padding-left: ${distance}" data-file-id="${item.id}">
            <i class="icon icon-control icon-add"></i>
            <i class="icon icon-file"></i>
            <span class="title">${item.name}</span>
          </div>
          ${treeHtml(fileData, item.id)}
        </li>`;
        });
        _html += '</ul>'; return _html;
    };
    function filesHandle(item) {
        tools.addEvent(item, 'click', function () {
            var treeNode_cur = tools.$('.treeNode-cur')[0];
            var fileId = item.dataset.fileId;
            var curElem = document.querySelector('.treeNode[data-file-id="' + fileId + '"]');
            var hasChild = hasChilds(fileData, fileId);
            var icon_control = tools.$('.icon-control', item)[0];
            var openStatus = true;
            tools.removeClass(treeNode_cur, 'treeNode-cur');
            tools.addClass(curElem, 'treeNode-cur');
            if (hasChild) {
                openStatus = tools.toggleClass(item.nextElementSibling, 'none');
                if (openStatus) { icon_control.className = 'icon icon-control icon-add'; } else { icon_control.className = 'icon icon-control icon-minus'; }
            }
        });
    };

})(window);