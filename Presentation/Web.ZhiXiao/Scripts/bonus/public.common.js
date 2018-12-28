!(function (doc, win) {
    var el = doc.documentElement;
    //resizeEvt = 'orientationchange' in window ? 'orientationchange' : 'resize';

    function setSize() {
        var w = el.clientWidth;
        if (!w) return;
        w = w > 480 ? 480 : w;
        w = w < 320 ? 320 : w;
        el.style.fontSize = (100 * (w / 1080)).toFixed(3) + 'px';
    }
    if (!doc.addEventListener) return;
    setSize();
    win.addEventListener('resize', setSize, false);
    win.addEventListener('pageshow', function (e) {
        if (e.persisted) {
            setSize();
        }
    }, false);

    // scroll top  
    $('.totop').tottTop({
        scrollTop: 100
    });
})(document, window);

function SAlert(msg) {
    Swal({
        position: 'top',
        padding: '0.3rem',
        title: msg,
        showConfirmButton: false,
        timer: 1000
      });
}

function loadingScrollData(container, url, params) {
    var isPreviousEventComplete = true, isDataAvailable = true;

    if (isPreviousEventComplete) {
        isPreviousEventComplete = false;
        $(".loading-img").show();

        $.ajax({
            type: "GET",
            url: url,
            data: params,
            success: function (result) {
                //callback();

                $(container).append(result);

                isPreviousEventComplete = true;

                $(".loading-img").hide();

                if (result === '') //When data is not available
                {
                    isDataAvailable = false;
                }
                return isDataAvailable;
            },
            error: function (error) {
                alert(error);
            }
        });
    }
}
