﻿class HgoFileManager {
    thisId;
    apiUrl;

    menuButtonContinerDiv;
    menuInputContinerDiv;
    jsSelectContinerDiv;
    breadcrumbContinerDiv;

    jsTree;
    jsSelect;
    contextMenu;
    lazyLoader;

    clipboard = [];
    browseHistory = [];

    backButton;
    pasteButton;

    static instances = [];

    constructor(id, apiEndPointUrl) {
        this.thisId = id;
        this.apiUrl = apiEndPointUrl;

        this.#initExplorer();
        this.lazyLoader = lozad();
        this.getFolderContent();

        HgoFileManager.instances.push(this);
    }

    destroy() {
        let i = HgoFileManager.instances.indexOf(this);
        HgoFileManager.instances.splice(i, 1);
    }

    static selectById(id) {
        if (id && HgoFileManager.instances && HgoFileManager.instances.length > 0) {
            let instance = HgoFileManager.instances.filter(function (el) {
                return el && el.thisId == "hgo_fm_" + id;
            });
            if (instance && instance.length > 0)
                return instance[0];
        }
    }

    #initMenu() {
        let menuContinerDiv = document.createElement('div');
        menuContinerDiv.classList.add('hgo-fm-menu');
        this.#fileManager().appendChild(menuContinerDiv);

        this.menuButtonContinerDiv = document.createElement('div');
        this.menuButtonContinerDiv.classList.add('button-continer');
        menuContinerDiv.appendChild(this.menuButtonContinerDiv);

        this.menuInputContinerDiv = document.createElement('div');
        this.menuInputContinerDiv.classList.add('input-continer');
        menuContinerDiv.appendChild(this.menuInputContinerDiv);

        this.backButton = this.addButtonToMenu('<i class="fa-solid fa-rotate-left"></i>', (e) => this.goPreviousPath());
        this.backButton.setAttribute('title', 'Back');
        this.backButton.setAttribute('disabled', 'disabled');
        this.addButtonToMenu('<i class="fa-solid fa-turn-up"></i>', (e) => this.goUpFolder()).setAttribute('title', 'Up');

        this.addButtonToMenu();

        this.addButtonToMenu('<i class="fa-solid fa-copy"></i>', (e) => this.copyItems()).setAttribute('title', 'Copy');
        this.addButtonToMenu('<i class="fa-solid fa-scissors"></i>', (e) => this.cutItems()).setAttribute('title', 'Cut');
        this.pasteButton = this.addButtonToMenu('<i class="fa-solid fa-paste"></i>', (e) => this.pasteItems());
        this.pasteButton.setAttribute('title', 'Paste');
        this.pasteButton.setAttribute('disabled', 'disabled');

        this.addButtonToMenu();

        this.addButtonToMenu('<i class="fa-solid fa-i-cursor"></i>', (e) => this.renameSelectedItems()).setAttribute('title', 'Rename');
        this.addButtonToMenu('<i class="fa-solid fa-file-pen"></i>', (e) => this.showFileContent()).setAttribute('title', 'Edit');
        this.addButtonToMenu('<i class="fa-solid fa-trash"></i>', (e) => this.deleteSelectedItems()).setAttribute('title', 'Delete');
        this.addButtonToMenu();
        this.addButtonToMenu('<i class="fa-solid fa-folder-plus"></i>', (e) => this.createNewFolder()).setAttribute('title', 'New Folder');
        this.addButtonToMenu('<i class="fa-solid fa-file-circle-plus"></i>', (e) => this.createNewFile()).setAttribute('title', 'New File');
        this.addButtonToMenu();
        this.addButtonToMenu('<i class="fa-solid fa-cloud-arrow-down"></i>', (e) => this.downloadFile()).setAttribute('title', 'Download');
        this.addButtonToMenu('<i class="fa-solid fa-cloud-arrow-up"></i>', (e) => this.showUploadPanel()).setAttribute('title', 'Upload');
        this.addButtonToMenu();
        this.addButtonToMenu('<i class="fa-solid fa-file-zipper"></i>', (e) => this.zipSelectedItems()).setAttribute('title', 'Zip');
        this.addButtonToMenu('<i class="fa-solid fa-square-arrow-up-right"></i>', (e) => this.extractSelectedItems()).setAttribute('title', 'UnZip');
        this.addButtonToMenu();
        this.addButtonToMenu('<i class="fa-solid fa-list-ul"></i>', (e) => this.toggleFilesListView()).setAttribute('title', 'Toggle View');

        this.addInputToMenu('Search ...', (e) => { if (e.keyCode == 13) this.search(e.currentTarget.value); }, 'search-box');
    }

    #initContextMenu() {
        let self = this;
        this.contextMenu = new ContextMenu(this.jsSelectContinerDiv, [
            { text: '<i class="fa-solid fa-turn-up"></i> Up', onclick: function (e) { self.goUpFolder(); } },
            { text: '<i class="fa-solid fa-repeat"></i> Reload', onclick: function (e) { self.getFolderContent(); } },
            null,
            { text: '<i class="fa-solid fa-copy"></i> Copy', onclick: function (e) { self.copyItems(); } },
            { text: '<i class="fa-solid fa-scissors"></i> Cut', onclick: function (e) { self.cutItems(); } },
            { text: '<i class="fa-solid fa-paste"></i> Paste', onclick: function (e) { self.pasteItems(); }, disabled: true },
            null,
            { text: '<i class="fa-solid fa-folder-plus"></i> New Folder', onclick: function (e) { self.createNewFolder(); } },
            { text: '<i class="fa-solid fa-file-circle-plus"></i> New File', onclick: function (e) { self.createNewFile(); } },
            null,
            { text: '<i class="fa-solid fa-i-cursor"></i> Rename', onclick: function (e) { self.renameSelectedItems(); } },
            { text: '<i class="fa-solid fa-file-pen"></i> Edit', onclick: function (e) { self.showFileContent(); } },
            { text: '<i class="fa-solid fa-trash"></i> Delete', onclick: function (e) { self.deleteSelectedItems(); } },
            null,
            { text: '<i class="fa-solid fa-file-zipper"></i> Zip', onclick: function (e) { self.zipSelectedItems(); } },
            { text: '<i class="fa-solid fa-square-arrow-up-right"></i> UnZip', onclick: function (e) { self.extractSelectedItems(); } },
            null,
            { text: '<i class="fa-solid fa-cloud-arrow-down"></i> Download', onclick: function (e) { self.downloadFile(); } },
            { text: '<i class="fa-solid fa-cloud-arrow-up"></i> Upload', onclick: function (e) { self.showUploadPanel(); } },
        ]);

        this.contextMenu.install();
    }

    #initExplorer() {
        let self = this;

        this.#fileManager().classList.add('hgo-fm-wrapper');

        //init top menu------------------------------------------------
        this.#initMenu();

        //create elements----------------------------------------------
        let wrapper = document.createElement('div');
        wrapper.classList.add('hgo-fm-content-wrapper');
        this.#fileManager().appendChild(wrapper);

        let jsTreeDiv = document.createElement('div');
        jsTreeDiv.classList.add('hgo-fm-tree');
        wrapper.appendChild(jsTreeDiv);

        this.jsSelectContinerDiv = document.createElement('div');
        this.jsSelectContinerDiv.classList.add('hgo-fm-fsitems');
        if (Cookies.get('hgo-fm-current-view-' + this.thisId))
            this.jsSelectContinerDiv.classList.add(Cookies.get('hgo-fm-current-view-' + this.thisId));
        this.jsSelectContinerDiv.addEventListener('dblclick', (e) => {
            e.stopPropagation();
            this.goUpFolder();
        });
        wrapper.appendChild(this.jsSelectContinerDiv);

        this.breadcrumbContinerDiv = document.createElement('div');
        this.breadcrumbContinerDiv.classList.add('hgo-fm-breadcrumb');
        this.#fileManager().appendChild(this.breadcrumbContinerDiv);

        let loadingPanelDiv = document.createElement('div');
        loadingPanelDiv.classList.add('loadingPanel');
        loadingPanelDiv.style.display = 'none';
        this.#fileManager().appendChild(loadingPanelDiv);

        let loaderDiv = document.createElement('div');
        loaderDiv.classList.add('loader');
        loadingPanelDiv.appendChild(loaderDiv);

        //init context menu--------------------------------------------
        this.#initContextMenu();

        //init JsTree--------------------------------------------------
        $(jsTreeDiv).jstree({
            "checkbox": {
                "keep_selected_style": false
            },
            "plugins": []
        }).on("select_node.jstree",
            function (evt, data) {
                if (data.event.type === 'click') {
                    if (data.node.parent === "#") {
                        //Click on root node
                        self.goUpFolder();
                    }
                    else {
                        //Click on child nodes
                        self.getFolderContent(self.getCurrentPath() + "\\" + data.node.text);
                    }
                }
            }
        );
        this.jsTree = $(jsTreeDiv).jstree(true);

        //init ViSelect------------------------------------------------
        this.jsSelect = new SelectionArea({
            selectables: ['#' + this.thisId + ' .hgo-fm-fsitems > .fsitem'],
            boundaries: [this.jsSelectContinerDiv]
        }).on('beforestart', ({ store, event }) => {
            let contextMenu = $('#' + this.thisId + ' .hgo-fm-fsitems .context');
            if (!event.ctrlKey && !event.metaKey && contextMenu.length === 0) {

                for (const el of store.stored) {
                    el.classList.remove('selected');
                }

                this.jsSelect.clearSelection();
            }
        }).on('move', ({ store: { changed: { added, removed } } }) => {
            for (const el of added) {
                el.classList.add('selected');
            }

            for (const el of removed) {
                el.classList.remove('selected');
            }
        }).on('beforedrag', ({ store, event }) => {
            if (!event.ctrlKey && !event.metaKey) {
                this.jsSelect.clearSelection();
            }
        });

        //init SplitJS-------------------------------------------------
        Split([jsTreeDiv, this.jsSelectContinerDiv], {
            sizes: [20, 80],
        });
    }

    #WaitingPanel() {
        let loadingPanel = this.#fileManager().querySelector('.loadingPanel');
        if (loadingPanel.style.display == 'none') {
            loadingPanel.style.display = 'block';
        }
        else {
            loadingPanel.style.display = 'none';
        }
    }

    #reloadBreadcrumb() {
        let splitedDirs = this.getCurrentPath().split('\\').filter(i => i);
        if (splitedDirs && splitedDirs.length > 0) {
            let currentDir = '';
            let continerUl = document.createElement('ul');
            for (let i = 0; i < splitedDirs.length; i++) {

                let dir = splitedDirs[i];
                currentDir += dir + "\\";
                let li = document.createElement('li');

                if (i < splitedDirs.length - 1) {

                    let btn = document.createElement('button');
                    btn.innerText = dir;
                    btn.setAttribute('path', currentDir);
                    btn.addEventListener('click', (e) => { this.getFolderContent(e.currentTarget.getAttribute('path')); });
                    li.appendChild(btn);

                }
                else {
                    li.innerText = dir;
                }

                continerUl.appendChild(li);
            }
            this.breadcrumbContinerDiv.innerHTML = '';
            this.breadcrumbContinerDiv.appendChild(continerUl);
        }
    }

    #fillFolderContent(data) {
        if (data.error) {
            this.showToastify(data.error);
        }

        if (!data.files || !data.dirs || !data.path) { return; }

        let currentPath = data.path.replace(/\\+$/, '') + "\\";
        this.#setCurrentPath(currentPath);

        let self = this;

        let selectDiv = this.jsSelectContinerDiv;
        selectDiv.innerHTML = '';

        let jsTreeNodes = [];

        $.each(data.dirs, function (idx, folderFullPath) {

            let dirName = folderFullPath.replace(currentPath, "");

            jsTreeNodes.push({ 'text': dirName, 'state': { 'opened': false, 'selected': false } });

            let fsItemDiv = document.createElement('div');
            fsItemDiv.setAttribute('title', dirName);
            fsItemDiv.setAttribute('path', folderFullPath);
            fsItemDiv.setAttribute('item-type', 'folder');
            fsItemDiv.classList.add('fsitem');
            fsItemDiv.addEventListener('dblclick', (e) => {
                e.stopPropagation();
                let path = e.currentTarget.getAttribute('path');
                self.getFolderContent(path);
            });

            let fsItemImg = document.createElement('img');
            fsItemImg.classList.add('lozad');
            fsItemImg.src = '/hgofilemanager/images/loading.gif';
            fsItemImg.setAttribute('data-src', '/hgofilemanager/images/folder.png');
            fsItemDiv.appendChild(fsItemImg);

            let fsItemSpan = document.createElement('span');
            fsItemSpan.innerText = dirName;
            fsItemDiv.appendChild(fsItemSpan);

            selectDiv.appendChild(fsItemDiv);
        });

        data.files.forEach((file) => {

            let fsItemDiv = document.createElement('div');
            fsItemDiv.setAttribute('title', file);
            fsItemDiv.setAttribute('path', currentPath + file);
            fsItemDiv.setAttribute('item-type', 'file');
            fsItemDiv.classList.add('fsitem');
            fsItemDiv.addEventListener('dblclick', (e) => {
                e.stopPropagation();
                self.downloadFile();
            });

            let fsItemImg = document.createElement('img');
            fsItemImg.classList.add('lozad');
            fsItemImg.src = '/hgofilemanager/images/loading.gif';
            fsItemImg.setAttribute('data-src', this.apiUrl + `?id=${this.thisId}&command=filePreview&parameters=${currentPath + file}`);
            fsItemDiv.appendChild(fsItemImg);

            let fsItemSpan = document.createElement('span');
            fsItemSpan.innerText = file;
            fsItemDiv.appendChild(fsItemSpan);

            selectDiv.appendChild(fsItemDiv);

        });

        this.jsTree.settings.core.data = [

            {
                'text': currentPath,
                'state': { 'opened': true, 'selected': false },
                'children': jsTreeNodes
            }
        ];
        this.jsTree.deselect_all(true);
        this.jsTree.refresh();
        this.#reloadBreadcrumb(currentPath);
        this.lazyLoader.observe();;
    }

    #checkIfAnyItemSelected(itemtype) {
        let selectedItems = this.getSelectedItemsPath(itemtype);
        if (!selectedItems) {
            this.showToastify('Please select your desired item(s).');
            return false;
        }
        return selectedItems;
    }

    #fileManager() {
        return document.getElementById(this.thisId);
    }

    #setCurrentPath(path) {
        if (path)
            Cookies.set('hgo-fm-current-path-' + this.thisId, path, { expires: 365 });
    }

    getCurrentPath = function () {
        let path = Cookies.get('hgo-fm-current-path-' + this.thisId);
        if (path)
            return path.replace(/\\+$/, '') + "\\";
        return "";
    }

    showToastify = function(text, type = 'error') {
        let bgColor = 'linear-gradient(to right, #b6ff8a, #238f00);';
        if (type === 'error') {
            bgColor = "linear-gradient(to right, #ff8a8a, #ff6565)";
        }

        Toastify({
            text: text,
            duration: 3000,
            selector: this.thisId,
            close: true,
            gravity: "bottom",
            position: "right",
            stopOnFocus: true,
            style: {
                background: bgColor,
                color: "#fff",
                position: "absolute"
            }
        }).showToast();
    }

    getSelectedItemsPath = function (itemtype) {
        let selectedItems = this.jsSelect.getSelection();
        if (selectedItems.length == 0) {
            return undefined;
        }

        let paths = [];
        for (let i = 0; i < selectedItems.length; i++) {
            if (!itemtype) {
                paths.push(selectedItems[i].getAttribute('path'));
            } else {
                if (selectedItems[i].getAttribute('item-type') == itemtype) {
                    paths.push(selectedItems[i].getAttribute('path'));
                }
            }
        }
        if (paths.length == 0) {
            return undefined;
        }

        return paths;
    }

    getAllItemsPath = function () {
        let allItems = $(this.jsSelectContinerDiv).find('.fsitem').map(function () {
            return $(this).attr("path");
        }).get();

        if (allItems.length > 0)
            return allItems;

        return undefined;
    }

    addButtonToMenu(innHtml, clickEventHandler, className) {
        if (innHtml) {
            let btn = document.createElement("button");
            btn.innerHTML = innHtml;
            if (clickEventHandler) btn.addEventListener('click', (e) => clickEventHandler(e));
            if (className) btn.classList.add(className);
            this.menuButtonContinerDiv.appendChild(btn);
            return btn;
        }
        let divider = document.createElement("hr");
        this.menuButtonContinerDiv.appendChild(divider);
        return divider;
    }

    addInputToMenu(placeholder, keydownEventHandler, className) {
        let input = document.createElement("input");
        input.setAttribute('placeholder', placeholder);
        if (keydownEventHandler) input.addEventListener('keydown', (e) => keydownEventHandler(e));
        if (className) input.classList.add(className);
        this.menuInputContinerDiv.appendChild(input);
    }

    getFolderContent = function (folder, addToHistory = true) {
        let self = this;

        if (!folder) {
            folder = this.getCurrentPath();
        } else {
            if (addToHistory === true) {
                if (this.browseHistory[this.browseHistory.length - 1] != this.getCurrentPath()) {
                    this.browseHistory.push(this.getCurrentPath());
                    this.backButton.removeAttribute('disabled');
                }
            }
        }

        folder = folder.replace(/\\+$/, '') + "\\";
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "getFolderContent", parameters: folder }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
        }, 'json');
    }

    goUpFolder = function () {
        let currentPath = this.getCurrentPath().replace(/\\+$/, '');
        if (currentPath) {
            this.getFolderContent(currentPath.substring(0, currentPath.lastIndexOf('\\')) + "\\");
        }
    }

    search = function (query) {
        let self = this;
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "Search", parameters: JSON.stringify({ Path: this.getCurrentPath(), Query: query }) }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
        }, 'json');
    }

    createNewFolder = function () {
        let newFolderName = prompt("Please enter folder name:", "New Folder");
        if (!newFolderName) {
            return;
        }

        let self = this;
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "CreateNewFolder", parameters: JSON.stringify({ Path: this.getCurrentPath(), FolderName: newFolderName }) }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
        }, 'json');
    }

    createNewFile = function () {
        let newFileName = prompt("Please enter your desired file name:", "New Text.txt");
        if (!newFileName) {
            return;
        }

        let self = this;
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "CreateNewFile", parameters: JSON.stringify({ Path: this.getCurrentPath(), FileName: newFileName }) }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
        }, 'json');
    }

    deleteSelectedItems = function () {
        let selectedItems = this.#checkIfAnyItemSelected();
        if (selectedItems === false) {
            return;
        }

        if (confirm("Are you sure you want to delete selected items?")) {
            let self = this;
            this.#WaitingPanel();
            $.post(this.apiUrl, { id: this.thisId, command: "DeleteItems", parameters: JSON.stringify({ Path: this.getCurrentPath(), Items: selectedItems }) }, function (data) {
                self.#WaitingPanel();
                self.#fillFolderContent(data);
            }, 'json');
        }
    }

    renameSelectedItems = function () {
        let selectedItems = this.#checkIfAnyItemSelected();
        if (selectedItems === false) {
            return;
        }

        let firstItemName = selectedItems[0].split("\\").pop();
        let newName = prompt("Please enter new name:", firstItemName);
        if (!newName) {
            return;
        }

        let currentPath = this.getCurrentPath();

        //check if already exist
        let allItems = this.getAllItemsPath();
        if (allItems)
            for (let i = 0; i < allItems.length; i++) {
                let element = allItems[i];
                if (element.toLowerCase() === (currentPath + newName).toLowerCase()) {
                    if (confirm("An item with '" + newName + "' already exist, do you want to overwrite?")) {
                        break;
                    } else {
                        return;
                    }
                }
            }

        let self = this;
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "RenameItems", parameters: JSON.stringify({ Path: currentPath, Items: selectedItems, NewName: newName }) }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
        }, 'json');
    }

    zipSelectedItems = function () {
        let selectedItems = this.#checkIfAnyItemSelected();
        if (selectedItems === false) {
            return;
        }

        let zipFileName = selectedItems[0].split("\\").pop() + ".zip";
        if (selectedItems.length > 1) {
            zipFileName = prompt("Please enter Zip file name:", zipFileName);
            if (!zipFileName) {
                return;
            }
        }

        let self = this;
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "ZipItems", parameters: JSON.stringify({ Path: this.getCurrentPath(), Items: selectedItems, FileName: zipFileName }) }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
        }, 'json');
    }

    extractSelectedItems = function () {
        let selectedItems = this.#checkIfAnyItemSelected();
        if (selectedItems === false) {
            return;
        }

        let self = this;
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "ExtractItems", parameters: JSON.stringify({ Path: this.getCurrentPath(), Items: selectedItems }) }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
        }, 'json');
    }

    showFileContent = function () {
        let selectedItems = this.#checkIfAnyItemSelected('file');
        if (selectedItems === false) {
            return;
        }

        for (let i = 0; i < selectedItems.length; i++) {
            window.open(`${this.apiUrl}?id=${this.thisId}&command=ShowFileContent&parameters=${selectedItems[i]}`, '_blank');
        }
    }

    downloadFile = function () {
        let selectedItems = this.#checkIfAnyItemSelected('file');
        if (selectedItems === false) {
            return;
        }

        for (let i = 0; i < selectedItems.length; i++) {
            window.open(`${this.apiUrl}?id=${this.thisId}&command=DownloadItem&parameters=${selectedItems[i]}`, '_blank');
        }
    }

    goPreviousPath = function () {
        if (this.browseHistory.length > 0) {
            this.getFolderContent(this.browseHistory[this.browseHistory.length - 1], false);
            this.browseHistory = this.browseHistory.slice(0, -1);
            if (this.browseHistory.length == 0) {
                this.backButton.setAttribute('disabled', 'disabled');
            }
        }
    }

    copyItems = function () {
        if (!this.#checkIfAnyItemSelected()) return;

        this.clipboard = [];
        this.clipboard.push("copy");

        this.clipboard = this.clipboard.concat(this.getSelectedItemsPath());
        this.pasteButton.removeAttribute('disabled');

        let pasteContextMenu = this.contextMenu.items.filter(function (el) {
            return el && el.text.includes('Paste');
        });
        pasteContextMenu[0].disabled = false;
    }

    cutItems = function () {
        if (!this.#checkIfAnyItemSelected()) return;

        this.clipboard = [];
        this.clipboard.push("cut");

        this.clipboard = this.clipboard.concat(this.getSelectedItemsPath());
        this.pasteButton.removeAttribute('disabled');

        let pasteContextMenu = this.contextMenu.items.filter(function (el) {
            return el && el.text.includes('Paste');
        });
        pasteContextMenu[0].disabled = false;
    }

    pasteItems = function () {
        if (this.clipboard.length > 1) {

            let action = this.clipboard[0];
            this.clipboard.shift();

            //check if already exist
            let currentPath = this.getCurrentPath();
            let allItems = this.getAllItemsPath();
            if (allItems)
                for (let j = 0; j < this.clipboard.length; j++) {
                    let newName = this.clipboard[j].split("\\").pop();
                    let breakLoop = false;
                    for (let i = 0; i < allItems.length; i++) {
                        let element = allItems[i];
                        if (element.toLowerCase() === (currentPath + newName).toLowerCase()) {
                            if (confirm("Some item(s) already exist, do you want to overwrite?")) {
                                breakLoop = true;
                                break;
                            } else {
                                this.clipboard = [action].concat(this.clipboard)
                                return;
                            }
                        }
                    }
                    if (breakLoop == true) break;
                }

            let self = this;
            this.#WaitingPanel();

            $.post(this.apiUrl, { id: this.thisId, command: action, parameters: JSON.stringify({ Path: this.getCurrentPath(), Items: this.clipboard }) }, function (data) {
                self.#WaitingPanel();
                self.#fillFolderContent(data);
            }, 'json');
        }
        this.clipboard = [];
        this.pasteButton.setAttribute('disabled', 'disabled');

        let pasteContextMenu = this.contextMenu.items.filter(function (el) {
            return el && el.text.includes('Paste');
        });
        pasteContextMenu[0].disabled = true;
    }

    toggleFilesListView = function () {
        if (this.jsSelectContinerDiv.classList.contains('list')) {
            this.jsSelectContinerDiv.classList.remove('list');
            Cookies.set('hgo-fm-current-view-' + this.thisId, '', { expires: 365 });
        }
        else {
            this.jsSelectContinerDiv.classList.add('list');
            Cookies.set('hgo-fm-current-view-' + this.thisId, 'list', { expires: 365 });
        }
    }

    showUploadPanel = function () {
        let uploadPanel = document.createElement('div');
        uploadPanel.classList.add('hgo-upload-panel', 'dropzone');

        let closeBtn = document.createElement('button');
        closeBtn.classList.add('close-button');
        closeBtn.innerText = 'Close';
        uploadPanel.appendChild(closeBtn);
        closeBtn.addEventListener('click', (e) => {
            uploadPanel.remove();
            this.getFolderContent();
        });

        this.#fileManager().appendChild(uploadPanel);

        //init DropZone (Uploader)-------------------------------------
        Dropzone.autoDiscover = false;
        let dropZone = new Dropzone(uploadPanel, {
            url: this.apiUrl + `?id=${this.thisId}&command=upload&parameters=${this.getCurrentPath()}`,
            parallelUploads: 1,
            chunking: true,
            chunkSize: 10000000, // bytes
            retryChunks: true,
            retryChunksLimit: 3,
            maxFilesize: 256, // bytes
            paramName: 'file',
            acceptedFiles: '' //'.png,.pdf'
        });
    }
}