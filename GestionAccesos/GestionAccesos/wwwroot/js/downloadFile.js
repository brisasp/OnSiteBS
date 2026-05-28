window.downloadFile = (fileName, contentType, content) => {
    console.log(content);
    var link = document.createElement('a');
    link.href = 'data:' + contentType + ';base64,' + content;
    link.download = fileName;
    link.click();
};