class HgoFileManager {
    thisId;
    apiUrl;
    config;

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
    cleanupButton;

    static instances = [];

    constructor(id, apiEndPointUrl, config) {
        this.thisId = id;
        this.apiUrl = apiEndPointUrl;
        this.config = config;

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
        let disabledFunctions = (typeof this.config.disabledFunctions != "undefined" ? this.config.disabledFunctions.join(',') : '').toLowerCase();

        let menuContinerDiv = document.createElement('div');
        menuContinerDiv.classList.add('hgo-fm-menu');
        this.#fileManager().appendChild(menuContinerDiv);

        this.menuButtonContinerDiv = document.createElement('div');
        this.menuButtonContinerDiv.classList.add('button-continer');
        menuContinerDiv.appendChild(this.menuButtonContinerDiv);

        this.menuInputContinerDiv = document.createElement('div');
        this.menuInputContinerDiv.classList.add('input-continer');
        menuContinerDiv.appendChild(this.menuInputContinerDiv);

        if (!disabledFunctions.includes('browse')) {
            this.backButton = this.addButtonToMenu('<i class="fa-solid fa-rotate-left"></i>', (e) => this.goPreviousPath());
            this.backButton.setAttribute('title', 'Back');
            this.backButton.setAttribute('disabled', 'disabled');

            this.addButtonToMenu('<i class="fa-solid fa-turn-up"></i>', (e) => this.goUpFolder()).setAttribute('title', 'Up');

            this.addButtonToMenu();
        }

        if (!disabledFunctions.includes('copy')) 
            this.addButtonToMenu('<i class="fa-solid fa-copy"></i>', (e) => this.copyItems()).setAttribute('title', 'Copy');
        if (!disabledFunctions.includes('cut')) 
            this.addButtonToMenu('<i class="fa-solid fa-scissors"></i>', (e) => this.cutItems()).setAttribute('title', 'Cut');
        if (!(disabledFunctions.includes('copy') && disabledFunctions.includes('cut'))) {
            this.pasteButton = this.addButtonToMenu('<i class="fa-solid fa-paste"></i>', (e) => this.pasteItems());
            this.pasteButton.setAttribute('title', 'Paste');
            this.pasteButton.setAttribute('disabled', 'disabled');

            this.addButtonToMenu();
        }

        if (!disabledFunctions.includes('rename')) 
            this.addButtonToMenu('<i class="fa-solid fa-i-cursor"></i>', (e) => this.renameSelectedItems()).setAttribute('title', 'Rename');
        if (!disabledFunctions.includes('getfilecontent')) 
            this.addButtonToMenu('<i class="fa-solid fa-file-pen"></i>', (e) => this.getFileContent()).setAttribute('title', 'Edit');
        if (!disabledFunctions.includes('delete')) 
            this.addButtonToMenu('<i class="fa-solid fa-trash"></i>', (e) => this.deleteSelectedItems()).setAttribute('title', 'Delete');
        if (!(disabledFunctions.includes('rename') && disabledFunctions.includes('getfilecontent') && disabledFunctions.includes('delete'))) 
            this.addButtonToMenu();

        if (!disabledFunctions.includes('createnewfolder'))
            this.addButtonToMenu('<i class="fa-solid fa-folder-plus"></i>', (e) => this.createNewFolder()).setAttribute('title', 'New Folder');
        if (!disabledFunctions.includes('createnewfile'))
            this.addButtonToMenu('<i class="fa-solid fa-file-circle-plus"></i>', (e) => this.createNewFile()).setAttribute('title', 'New File');
        if (!(disabledFunctions.includes('createnewfolder') && disabledFunctions.includes('createnewfile'))) 
            this.addButtonToMenu();

        if (!disabledFunctions.includes('view'))
            this.addButtonToMenu('<i class="fa-solid fa-eye"></i>', (e) => this.viewFile()).setAttribute('title', 'View');
        if (!disabledFunctions.includes('download'))
            this.addButtonToMenu('<i class="fa-solid fa-cloud-arrow-down"></i>', (e) => this.downloadFile()).setAttribute('title', 'Download');
        if (!disabledFunctions.includes('upload'))
            this.addButtonToMenu('<i class="fa-solid fa-cloud-arrow-up"></i>', (e) => this.showUploadPanel()).setAttribute('title', 'Upload');
        if (!disabledFunctions.includes('cleanupuploads')) {
            this.cleanupButton = this.addButtonToMenu('<i class="fa-solid fa-broom"></i>', (e) => this.cleanupFailedUploads());
            this.cleanupButton.setAttribute('title', 'Clean Failed Uploads');
        }
        if (!(disabledFunctions.includes('download') && disabledFunctions.includes('upload') && disabledFunctions.includes('view') && disabledFunctions.includes('cleanupuploads')))
            this.addButtonToMenu();

        if (!disabledFunctions.includes('zip')) 
            this.addButtonToMenu('<i class="fa-solid fa-file-zipper"></i>', (e) => this.zipSelectedItems()).setAttribute('title', 'Zip');
        if (!disabledFunctions.includes('unzip')) 
            this.addButtonToMenu('<i class="fa-solid fa-square-arrow-up-right"></i>', (e) => this.extractSelectedItems()).setAttribute('title', 'UnZip');
        if (!(disabledFunctions.includes('unzip') && disabledFunctions.includes('zip'))) 
            this.addButtonToMenu();

        if (!disabledFunctions.includes('toggleview')) 
            this.addButtonToMenu('<i class="fa-solid fa-list-ul"></i>', (e) => this.toggleFilesListView()).setAttribute('title', 'Toggle View');

        if (!disabledFunctions.includes('search'))
            this.addInputToMenu('Search ...', (e) => { if (e.keyCode == 13) this.search(e.currentTarget.value); }, 'search-box');

        if (disabledFunctions.includes('menubar')) 
            menuContinerDiv.style.display = 'none';
    }

    #initContextMenu() {
        let disabledFunctions = (typeof this.config.disabledFunctions != "undefined" ? this.config.disabledFunctions.join(',') : '').toLowerCase();
        let self = this;
        let menuItems = [];

        if (!disabledFunctions.includes('browse')) 
            menuItems.push({ text: '<i class="fa-solid fa-turn-up"></i> Up', onclick: function (e) { self.goUpFolder(); } });
        if (!disabledFunctions.includes('reload')) 
            menuItems.push({ text: '<i class="fa-solid fa-repeat"></i> Reload', onclick: function (e) { self.getFolderContent(); } });
        if (!(disabledFunctions.includes('browse') && disabledFunctions.includes('reload'))) 
            menuItems.push(null);

        if (!disabledFunctions.includes('copy')) 
            menuItems.push({ text: '<i class="fa-solid fa-copy"></i> Copy', onclick: function (e) { self.copyItems(); } });
        if (!disabledFunctions.includes('cut')) 
            menuItems.push({ text: '<i class="fa-solid fa-scissors"></i> Cut', onclick: function (e) { self.cutItems(); } });
        if (!(disabledFunctions.includes('copy') && disabledFunctions.includes('cut'))) {
            menuItems.push({ text: '<i class="fa-solid fa-paste"></i> Paste', onclick: function (e) { self.pasteItems(); }, disabled: true });
            menuItems.push(null);
        }

        if (!disabledFunctions.includes('createnewfolder'))
            menuItems.push({ text: '<i class="fa-solid fa-folder-plus"></i> New Folder', onclick: function (e) { self.createNewFolder(); } });
        if (!disabledFunctions.includes('createnewfile'))
            menuItems.push({ text: '<i class="fa-solid fa-file-circle-plus"></i> New File', onclick: function (e) { self.createNewFile(); } });
        if (!(disabledFunctions.includes('createnewfolder') && disabledFunctions.includes('createnewfile')))
            menuItems.push(null);

        if (!disabledFunctions.includes('rename')) 
            menuItems.push({ text: '<i class="fa-solid fa-i-cursor"></i> Rename', onclick: function (e) { self.renameSelectedItems(); } });
        if (!disabledFunctions.includes('getfilecontent'))
            menuItems.push({ text: '<i class="fa-solid fa-file-pen"></i> Edit', onclick: function (e) { self.getFileContent(); } });
        if (!disabledFunctions.includes('delete')) 
            menuItems.push({ text: '<i class="fa-solid fa-trash"></i> Delete', onclick: function (e) { self.deleteSelectedItems(); } });
        if (!(disabledFunctions.includes('rename') && disabledFunctions.includes('getfilecontent') && disabledFunctions.includes('delete'))) 
            menuItems.push(null);

        if (!disabledFunctions.includes('zip')) 
            menuItems.push({ text: '<i class="fa-solid fa-file-zipper"></i> Zip', onclick: function (e) { self.zipSelectedItems(); } });
        if (!disabledFunctions.includes('unzip')) 
            menuItems.push({ text: '<i class="fa-solid fa-square-arrow-up-right"></i> UnZip', onclick: function (e) { self.extractSelectedItems(); } });
        if (!(disabledFunctions.includes('unzip') && disabledFunctions.includes('zip'))) 
            menuItems.push(null);

        if (!disabledFunctions.includes('view'))
            menuItems.push({ text: '<i class="fa-solid fa-eye"></i> View', onclick: function (e) { self.viewFile(); } });
        if (!disabledFunctions.includes('download')) 
            menuItems.push({ text: '<i class="fa-solid fa-cloud-arrow-down"></i> Download', onclick: function (e) { self.downloadFile(); } });
        if (!disabledFunctions.includes('upload')) 
            menuItems.push({ text: '<i class="fa-solid fa-cloud-arrow-up"></i> Upload', onclick: function (e) { self.showUploadPanel(); } });


        this.contextMenu = new ContextMenu(this.jsSelectContinerDiv, menuItems);

        if (!disabledFunctions.includes('contextmenu'))
            this.contextMenu.install();
    }

    #initExplorer() {
        let self = this;
        let disabledFunctions = (typeof this.config.disabledFunctions != "undefined" ? this.config.disabledFunctions.join(',') : '').toLowerCase();
        const isTouchDevice = window.matchMedia('(pointer: coarse)').matches;

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
        if (!disabledFunctions.includes('browse')) 
            this.jsSelectContinerDiv.addEventListener('dblclick', (e) => {
                e.stopPropagation();
                this.goUpFolder();
            });
        if (!disabledFunctions.includes('upload')) {
            this.jsSelectContinerDiv.addEventListener('dragover', (e) => {
                if (e.dataTransfer.types.length > 0 && e.dataTransfer.types[0] == "Files" &&
                    e.dataTransfer.items.length > 0) {
                    e.stopPropagation();
                    this.showUploadPanel();
                }
            });
        }
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
        if (!isTouchDevice) {
            $(jsTreeDiv).jstree({
                "checkbox": {
                    "keep_selected_style": false
                },
                "plugins": []
            }).on("select_node.jstree", (!disabledFunctions.includes('browse')) ?
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
                : null
            );
            this.jsTree = $(jsTreeDiv).jstree(true);
            if (disabledFunctions.includes('folderstree'))
                jsTreeDiv.style.display = 'none';
        } else {
            this.jsTree = null;
            if (!disabledFunctions.includes('folderstree'))
                jsTreeDiv.style.display = 'none';
        }

        //init ViSelect------------------------------------------------
        if (!isTouchDevice) {
            this.jsSelect = new SelectionArea({
                selectables: ['#' + this.thisId + ' .hgo-fm-fsitems > .fsitem'],
                boundaries: [this.jsSelectContinerDiv],
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
        } else {
            this.jsSelect = null;
        }

        //init SplitJS-------------------------------------------------
        if (!disabledFunctions.includes('folderstree') && !isTouchDevice)
            Split([jsTreeDiv, this.jsSelectContinerDiv], {
                sizes: [20, 80],
            });
        if (isTouchDevice && !disabledFunctions.includes('folderstree')) {
            jsTreeDiv.style.width = '0';
            jsTreeDiv.style.display = 'none';
            this.jsSelectContinerDiv.style.width = '100%';
        }
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
        let disabledFunctions = (typeof this.config.disabledFunctions != "undefined" ? this.config.disabledFunctions.join(',') : '').toLowerCase();
        if (disabledFunctions.includes('breadcrumb')) {
            this.breadcrumbContinerDiv.style.display = 'none';
            return;
        }

        this.breadcrumbContinerDiv.style.display = 'initial';

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
                    if (!disabledFunctions.includes('browse'))
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
        let disabledFunctions = (typeof this.config.disabledFunctions != "undefined" ? this.config.disabledFunctions.join(',') : '').toLowerCase();

        if (data.Error) {
            this.showToastify(data.Error);
            return;
        }

        if (!data.Files || !data.Folders || !data.CurrentPath) { return; }

        let currentPath = data.CurrentPath.replace(/\\+$/, '') + "\\";
        this.#setCurrentPath(currentPath);

        let self = this;
        let selectDiv = this.jsSelectContinerDiv;
        selectDiv.innerHTML = '';

        // Add touch handling to container to prevent selection during scroll
        let containerTouchStartX = 0;
        let containerTouchStartY = 0;
        let containerTouchStartTime = 0;
        let isScrolling = false;

        selectDiv.addEventListener('touchstart', (event) => {
            containerTouchStartX = event.touches[0].clientX;
            containerTouchStartY = event.touches[0].clientY;
            containerTouchStartTime = Date.now();
            isScrolling = false;
        }, { passive: true });

        selectDiv.addEventListener('touchmove', (event) => {
            const touchDistance = Math.sqrt(
                Math.pow(event.touches[0].clientX - containerTouchStartX, 2) +
                Math.pow(event.touches[0].clientY - containerTouchStartY, 2)
            );
            if (touchDistance > 10) {
                isScrolling = true;
            }
        }, { passive: true });

        selectDiv.addEventListener('touchend', () => {
            isScrolling = false;
        }, { passive: true });

        const addSelectionHandlers = (item) => {
            let lastClickTime = 0;
            let isMobile = window.matchMedia('(pointer: coarse)').matches;
            let touchStartTime = 0;
            let touchStartX = 0;
            let touchStartY = 0;

            item.addEventListener('touchstart', (event) => {
                touchStartTime = Date.now();
                touchStartX = event.touches[0].clientX;
                touchStartY = event.touches[0].clientY;
            }, { passive: true });

            item.addEventListener('touchend', (event) => {
                // If container is scrolling, don't handle as tap
                if (isScrolling) return;

                const touchDuration = Date.now() - touchStartTime;
                const touchEndX = event.changedTouches[0].clientX;
                const touchEndY = event.changedTouches[0].clientY;
                const touchDistance = Math.sqrt(Math.pow(touchEndX - touchStartX, 2) + Math.pow(touchEndY - touchStartY, 2));

                // If it was a scroll (moved more than 10px or took longer than 500ms), don't treat as tap
                if (touchDistance > 10 || touchDuration > 500) {
                    return;
                }

                const alreadySelected = item.classList.contains('selected');
                const itemType = item.getAttribute('item-type');
                const itemPath = item.getAttribute('path');

                const currentSelection = Array.from(selectDiv.querySelectorAll('.fsitem.selected'));
                if (currentSelection.length > 0 && !currentSelection.includes(item)) {
                    currentSelection.forEach((selectedItem) => selectedItem.classList.remove('selected'));
                }
                item.classList.toggle('selected', !alreadySelected);

                const now = Date.now();
                if (now - lastClickTime < 300 && alreadySelected) {
                    if (itemType === 'folder') {
                        self.getFolderContent(itemPath);
                    } else if (!disabledFunctions.includes('view')) {
                        self.viewFile();
                    }
                }
                lastClickTime = now;
            }, { passive: true });

            item.addEventListener('click', (event) => {
                if (isMobile) return;
                const alreadySelected = item.classList.contains('selected');
                const itemType = item.getAttribute('item-type');
                const itemPath = item.getAttribute('path');

                if (event.ctrlKey || event.metaKey || event.shiftKey) {
                    item.classList.toggle('selected', !alreadySelected);
                    return;
                }

                const otherSelectedItems = Array.from(selectDiv.querySelectorAll('.fsitem.selected'));
                otherSelectedItems.forEach((selectedItem) => {
                    if (selectedItem !== item) selectedItem.classList.remove('selected');
                });
                item.classList.add('selected');
            });

            item.addEventListener('dblclick', (event) => {
                if (isMobile) return;
                event.stopPropagation();
                const itemType = item.getAttribute('item-type');
                const itemPath = item.getAttribute('path');
                if (itemType === 'folder') {
                    self.getFolderContent(itemPath);
                } else if (!disabledFunctions.includes('view')) {
                    self.viewFile();
                }
            });
        };

        let jsTreeNodes = [];
        const normalizedCurrentPath = currentPath.replace(/\\+$/, '');
        const parentPath = normalizedCurrentPath.lastIndexOf('\\') >= 0
            ? normalizedCurrentPath.substring(0, normalizedCurrentPath.lastIndexOf('\\')) + "\\"
            : currentPath;

        if (normalizedCurrentPath && normalizedCurrentPath.lastIndexOf('\\') >= 0) {
            const parentItem = document.createElement('div');
            parentItem.setAttribute('title', 'پوشه قبلی');
            parentItem.setAttribute('path', parentPath);
            parentItem.setAttribute('item-type', 'folder');
            parentItem.classList.add('fsitem', 'fsitem-parent');
            let parentLastClickTime = 0;
            let parentTouchStartTime = 0;
            let parentTouchStartX = 0;
            let parentTouchStartY = 0;

            parentItem.addEventListener('touchstart', (event) => {
                parentTouchStartTime = Date.now();
                parentTouchStartX = event.touches[0].clientX;
                parentTouchStartY = event.touches[0].clientY;
            }, { passive: true });

            parentItem.addEventListener('touchend', (event) => {
                // If container is scrolling, don't handle as tap
                if (isScrolling) return;

                const touchDuration = Date.now() - parentTouchStartTime;
                const touchEndX = event.changedTouches[0].clientX;
                const touchEndY = event.changedTouches[0].clientY;
                const touchDistance = Math.sqrt(Math.pow(touchEndX - parentTouchStartX, 2) + Math.pow(touchEndY - parentTouchStartY, 2));

                if (touchDistance > 10 || touchDuration > 500) {
                    return;
                }

                const alreadySelected = parentItem.classList.contains('selected');
                const currentSelection = Array.from(selectDiv.querySelectorAll('.fsitem.selected'));
                currentSelection.forEach((selectedItem) => selectedItem.classList.remove('selected'));
                parentItem.classList.toggle('selected', !alreadySelected);

                const now = Date.now();
                if (now - parentLastClickTime < 300 && alreadySelected) {
                    self.getFolderContent(parentPath);
                }
                parentLastClickTime = now;
            }, { passive: true });

            parentItem.addEventListener('click', (event) => {
                if (window.matchMedia('(pointer: coarse)').matches) return;
                const alreadySelected = parentItem.classList.contains('selected');
                const currentSelection = Array.from(selectDiv.querySelectorAll('.fsitem.selected'));
                currentSelection.forEach((selectedItem) => selectedItem.classList.remove('selected'));
                parentItem.classList.add('selected');
            });

            parentItem.addEventListener('dblclick', (event) => {
                if (window.matchMedia('(pointer: coarse)').matches) return;
                event.stopPropagation();
                self.getFolderContent(parentPath);
            });

            const basePath = window.HGOFM_BASEPATH || '';
            const fsItemImg = document.createElement('img');
            fsItemImg.classList.add('lozad');
            fsItemImg.src = basePath + '/hgofilemanager/images/loading.gif';
            fsItemImg.setAttribute('data-src', basePath + '/hgofilemanager/images/folderBack64.png');
            parentItem.appendChild(fsItemImg);

            const fsItemSpan = document.createElement('span');
            fsItemSpan.innerText = '(پوشه قبلی)';
            parentItem.appendChild(fsItemSpan);

            selectDiv.appendChild(parentItem);
        }

        $.each(data.Folders, function (idx, folder) {

            jsTreeNodes.push({ 'text': folder.FolderName, 'state': { 'opened': false, 'selected': false } });

            let fsItemDiv = document.createElement('div');
            fsItemDiv.setAttribute('title', 'Name: ' + folder.FolderName + '\nCreate Date: ' + folder.CreateDate + '\nModified Date: ' + folder.ModifiedDate);
            fsItemDiv.setAttribute('path', folder.VirtualPath);
            fsItemDiv.setAttribute('item-type', 'folder');
            fsItemDiv.classList.add('fsitem');
            addSelectionHandlers(fsItemDiv);
            if (!disabledFunctions.includes('browse'))
                fsItemDiv.addEventListener('dblclick', (e) => {
                    e.stopPropagation();
                    let path = e.currentTarget.getAttribute('path');
                    self.getFolderContent(path);
                });

            let basePath = window.HGOFM_BASEPATH || '';
            let fsItemImg = document.createElement('img');
            fsItemImg.classList.add('lozad');
            fsItemImg.src = basePath + '/hgofilemanager/images/loading.gif';
            fsItemImg.setAttribute('data-src', basePath + '/hgofilemanager/images/folder.png');
            fsItemDiv.appendChild(fsItemImg);

            let fsItemSpan = document.createElement('span');
            fsItemSpan.innerText = folder.FolderName;
            fsItemDiv.appendChild(fsItemSpan);

            selectDiv.appendChild(fsItemDiv);
        });

        data.Files.forEach((file) => {

            let fsItemDiv = document.createElement('div');
            fsItemDiv.setAttribute('title', 'Name: ' + file.FileName + '\nSize: ' + file.FileSize + '\nCreate Date: ' + file.CreateDate + '\nModified Date: ' + file.ModifiedDate);
            fsItemDiv.setAttribute('path', file.VirtualPath);
            fsItemDiv.setAttribute('item-type', 'file');
            fsItemDiv.classList.add('fsitem');
            addSelectionHandlers(fsItemDiv);

            if (!disabledFunctions.includes('view')) 
                fsItemDiv.addEventListener('dblclick', (e) => {
                    e.stopPropagation();
                    self.viewFile();
                });

            let basePath = window.HGOFM_BASEPATH || '';
            let fsItemImg = document.createElement('img');
            fsItemImg.classList.add('lozad');
            fsItemImg.src = basePath + '/hgofilemanager/images/loading.gif';
            if (!disabledFunctions.includes('filepreview')) {
                fsItemImg.setAttribute('data-src', this.apiUrl + `?id=${this.thisId}&command=filePreview&parameters=${encodeURIComponent(file.VirtualPath)}`);
            } else {
                fsItemImg.setAttribute('data-src', basePath + '/hgofilemanager/images/file.png');
            }
            fsItemDiv.appendChild(fsItemImg);

            let fsItemSpan = document.createElement('span');
            fsItemSpan.innerText = file.FileName;
            fsItemDiv.appendChild(fsItemSpan);

            let fsItemSizeSpan = document.createElement('span');
            fsItemSizeSpan.innerText = file.FileSize;
            fsItemSizeSpan.classList.add('file-size');
            fsItemSpan.appendChild(fsItemSizeSpan);

            selectDiv.appendChild(fsItemDiv);

        });

        let treeData = [];
        if (normalizedCurrentPath && normalizedCurrentPath.lastIndexOf('\\') >= 0) {
            treeData.push({
                'text': 'پوشه قبلی',
                'state': { 'opened': false, 'selected': false },
                'data': { 'isParent': true, 'path': parentPath },
                'li_attr': { 'class': 'jstree-parent-folder' }
            });
        }
        treeData.push({
            'text': currentPath,
            'state': { 'opened': true, 'selected': false },
            'children': jsTreeNodes
        });
        if (this.jsTree) {
            this.jsTree.settings.core.data = treeData;
            this.jsTree.deselect_all(true);
            this.jsTree.refresh();
        }
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
        let selectedItems = this.jsSelect && typeof this.jsSelect.getSelection === 'function'
            ? this.jsSelect.getSelection()
            : [];

        if (!selectedItems || selectedItems.length === 0) {
            selectedItems = Array.from(this.jsSelectContinerDiv.querySelectorAll('.fsitem.selected'));
        }

        if (!selectedItems || selectedItems.length === 0) {
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
                    if (this.backButton)
                        this.backButton.removeAttribute('disabled');
                }
            }
        }

        folder = folder.replace(/\\+$/, '') + "\\";
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "getFolderContent", parameters: folder }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
            self.#updateCleanupButton(data.FailedUploadsCount || 0);
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
            data.IsSearchResult = true;
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
            $.post(this.apiUrl, { id: this.thisId, command: "delete", parameters: JSON.stringify({ Path: this.getCurrentPath(), Items: selectedItems }) }, function (data) {
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
        $.post(this.apiUrl, { id: this.thisId, command: "rename", parameters: JSON.stringify({ Path: currentPath, Items: selectedItems, NewName: newName }) }, function (data) {
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
        $.post(this.apiUrl, { id: this.thisId, command: "zip", parameters: JSON.stringify({ Path: this.getCurrentPath(), Items: selectedItems, FileName: zipFileName }) }, function (data) {
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
        $.post(this.apiUrl, { id: this.thisId, command: "unzip", parameters: JSON.stringify({ Path: this.getCurrentPath(), Items: selectedItems }) }, function (data) {
            self.#WaitingPanel();
            self.#fillFolderContent(data);
        }, 'json');
    }

    getFileContent = function () {
        let selectedItems = this.#checkIfAnyItemSelected('file');
        if (selectedItems === false) {
            return;
        }

        for (let i = 0; i < selectedItems.length; i++) {
            window.open(`${this.apiUrl}?id=${this.thisId}&command=getfilecontent&parameters=${encodeURIComponent(selectedItems[i])}`, '_blank');
        }
    }

    downloadFile = function () {
        let selectedItems = this.#checkIfAnyItemSelected('file');
        if (selectedItems === false) {
            return;
        }

        for (let i = 0; i < selectedItems.length; i++) {
            window.open(`${this.apiUrl}?id=${this.thisId}&command=download&parameters=${encodeURIComponent(selectedItems[i])}`, '_blank');
        }
    }

    viewFile = function () {
        let selectedItems = this.#checkIfAnyItemSelected('file');
        if (selectedItems === false) {
            return;
        }

        for (let i = 0; i < selectedItems.length; i++) {
            window.open(`${this.apiUrl}?id=${this.thisId}&command=view&parameters=${encodeURIComponent(selectedItems[i])}`, '_blank');
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
        if (this.pasteButton)
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
        if (this.pasteButton)
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
        if (this.pasteButton)
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
            // Cancel all ongoing uploads before closing
            if (dropZone.getUploadingFiles().length > 0) {
                dropZone.getUploadingFiles().forEach(file => {
                    dropZone.cancelUpload(file);
                });
            }
            uploadPanel.remove();
            this.getFolderContent();
        });

        this.#fileManager().appendChild(uploadPanel);

        //init DropZone (Uploader)-------------------------------------
        Dropzone.autoDiscover = false;
        let dropZone = new Dropzone(uploadPanel, {
            url: this.apiUrl + `?id=${this.thisId}&command=upload&parameters=${encodeURIComponent(this.getCurrentPath())}`,
            parallelUploads: (typeof this.config.parallelUploads != "undefined" ? this.config.parallelUploads : 1),
            chunking: (typeof this.config.chunking != "undefined" ? this.config.chunking : true),
            chunkSize: (typeof this.config.chunkSizeByte != "undefined" ? this.config.chunkSizeByte : 10000000),  // bytes
            retryChunks: (typeof this.config.retryChunks != "undefined" ? this.config.retryChunks : true),
            retryChunksLimit: (typeof this.config.retryChunksLimit != "undefined" ? this.config.retryChunksLimit : 3),
            maxFilesize: (typeof this.config.maxFileSizeToUploadMByte != "undefined" ? this.config.maxFileSizeToUploadMByte : 256), // mega bytes
            acceptedFiles: (typeof this.config.acceptedFiles != "undefined" ? this.config.acceptedFiles : ''), //'.png,.pdf'
            paramName: 'file',
        });

        // Refresh folder content when upload completes successfully
        dropZone.on("success", (file) => {
            // If this was a chunked upload, finalize it
            if (file.upload.chunked) {
                $.post(this.apiUrl, { id: this.thisId, command: "finalizeupload", parameters: JSON.stringify({ fileName: file.name, path: this.getCurrentPath() }) });
            }
            this.getFolderContent();
        });

        // Handle upload errors
        dropZone.on("error", (file, errorMessage) => {
            this.showToastify('Upload failed: ' + errorMessage);
        });

        // Handle cancellation
        dropZone.on("canceled", (file) => {
            this.showToastify('Upload cancelled');
        });
    }

    #updateCleanupButton(count) {
        if (this.cleanupButton) {
            if (count > 0) {
                this.cleanupButton.innerHTML = `<i class="fa-solid fa-broom"></i> (${count})`;
                this.cleanupButton.removeAttribute('disabled');
            } else {
                this.cleanupButton.innerHTML = '<i class="fa-solid fa-broom"></i>';
                this.cleanupButton.setAttribute('disabled', 'disabled');
            }
        }
    }

    cleanupFailedUploads = function () {
        let self = this;
        this.#WaitingPanel();
        $.post(this.apiUrl, { id: this.thisId, command: "cleanupuploads", parameters: this.getCurrentPath() }, function (data) {
            self.#WaitingPanel();
            if (data.DeletedCount > 0) {
                self.showToastify(`Cleaned up ${data.DeletedCount} failed upload(s)`, 'success');
            } else {
                self.showToastify('No failed uploads to clean');
            }
            self.getFolderContent();
        }, 'json');
    }
}