$(document).ready(function () {
    $("#BrowserMode input:checked").parent("td").addClass("SelectedPager");
    $('#dnntreeTabs li .rtIn, #dnntreeTabs li .rtImg').click(function () {
        $('#panelLoading').show();
    });
    SwitchView($("#ListViewState").val());
    $('input[type="submit"],.LinkNormal,.LinkDisabled').button();
    $(".Toolbar").buttonset();
    //$("form").form();
    //$('select').not('#AnchorList, #LanguageList, #dDlQuality').selectgroup();
    $('#txtWidth,#txtHeight').spinner();
    $('#BrowserMode td').addClass('ui-state-default ui-corner-top');
    $('#BrowserMode label').addClass('ui-tabs-anchor');
    $('.SelectedPager').addClass('ui-tabs-active ui-state-active ui-state-focus');
    $("#BrowserMode td").hover(

    function () {
        $(this).addClass("ui-state-hover");
    },

    function () {
        $(this).removeClass("ui-state-hover");
    });
});

function SwitchView(css) {
    $("#ListViewState").val(css);
    var list = $("#FilesBox ul");
    list.attr('class', 'Files' + css);
    $('.SwitchDetailView').css('font-weight', 'normal');
    $('.SwitchListView').css('font-weight', 'normal');
    $('.SwitchIconsView').css('font-weight', 'normal');
    $('.Switch' + css).css('font-weight', 'bold');
}