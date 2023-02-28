using Blazor.DownloadFileFast.Interfaces;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Client.Models;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using System.IO.Compression;
using System.Net.Http.Json;
using System.Text;

namespace PiratenKarte.Client.Pages.MapObjects;

public partial class List {
    [CascadingParameter]
    public required IModalService Modal { get; init; }

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required IBlazorDownloadFileService FileService { get; init; }
    [Inject]
    public required AppSettings Settings { get; init; }
    [Inject]
    public required ParameterPassService Params { get; init; }

    protected override string PermissionFilter => "objects_read";

    private int Page;
    private int ItemsPerPage = 10;
    private int TotalItems;

    private PagedData<MapObject>? Objects;
    private bool[]? CheckboxValues;

    private bool Submitting;

    private bool _checkAll;
    private bool CheckAll {
        get => _checkAll;
        set {
            _checkAll = value;

            if (CheckboxValues != null) {
                for (int i = 0; i < CheckboxValues.Length; i++)
                    CheckboxValues[i] = value;

                StateHasChanged();
            }
        }
    }

    protected override async Task OnInitializedAsync() {
        ItemsPerPage = StateService.Current.ItemsPerPage;

        await Reload();
        await base.OnInitializedAsync();
    }

    private async Task ChangePage(int page) {
        StateService.Current.ItemsPerPage = ItemsPerPage;
        StateService.Write();

        Page = page;
        await Reload();
    }

    private async Task Reload() {
        Submitting = true;
        Objects = await Http.GetFromJsonAsync<PagedData<MapObject>>(
            $"MapObjects/GetPaged?page={Page}&itemsPerPage={ItemsPerPage}");
        CheckboxValues = new bool[Objects?.TotalCount ?? 1];

        TotalItems = Objects?.TotalCount ?? 1;

        _checkAll = false;
        Submitting = false;
        StateHasChanged();
    }

    private async Task DeleteOne(Guid id) {
        var obj = Objects!.Data.Find(obj => obj.Id == id);

        var param = new ModalParameters()
            .Add(nameof(ConfirmModal.Title), "Löschen Bestätigen")
            .Add(nameof(ConfirmModal.Content), $"Soll \"{obj!.Name}\" wirklich gelöscht werden?");
        var confirmModal = Modal.Show<ConfirmModal>("", param);
        var result = await confirmModal.Result;

        if (result.Cancelled)
            return;

        Submitting = true;
        await Http.PostAsJsonAsync("MapObjects/Delete", id);

        Submitting = false;
        await Reload();
    }

    private async Task DeleteMany() {
        if (Objects == null || CheckboxValues == null)
            return;

        var deleteList = SelectedObjects().Select(mo => mo.Id);
        var sb = new StringBuilder();

        sb.AppendLine("<span>Sollen alle folgenden Objekte gelöscht werden?</span>");
        sb.AppendLine("<ul>");
        foreach (var objId in deleteList) {
            var obj = Objects.Data.Find(o => o.Id == objId);
            sb.Append("<li>").Append(obj!.Name).AppendLine("</li>");
        }
        sb.AppendLine("</ul>");

        var param = new ModalParameters()
            .Add(nameof(ConfirmModal.Title), "Löschen Bestätigen")
            .Add(nameof(ConfirmModal.Content), sb.ToString());
        var confirmModal = Modal.Show<ConfirmModal>("", param);
        var result = await confirmModal.Result;

        if (result.Cancelled)
            return;

        Submitting = true;
        await Http.PostAsJsonAsync("MapObjects/DeleteMany", deleteList);

        Submitting = false;
        await Reload();
    }

    private async Task DownloadQRCodeOne(Guid id) {
        await FileService.DownloadFileAsync(id.ToString() + ".png", QrCodeGenerator.Generate(id, Settings.Domain));
    }

    private async Task DownloadQRCodeMany() {
        using var memStream = new MemoryStream();
        using (var zip = new ZipArchive(memStream, ZipArchiveMode.Create, true)) {
            foreach (var obj in SelectedObjects()) {
                var entry = zip.CreateEntry(obj.Id.ToString() + ".png");
                using var fileStream = entry.Open();

                var data = QrCodeGenerator.Generate(obj.Id, Settings.Domain);
                fileStream.Write(data, 0, data.Length);
            }
        }

        memStream.Seek(0, SeekOrigin.Begin);
        await FileService.DownloadFileAsync("qrCodes.zip", memStream.ToArray());
    }

    private void MoveManyToStorage() {
        Params.Put("MoveManyToStorage.Objects", SelectedObjects().ToList());
        NavManager.NavigateTo("/mapobjects/movemanytostorage");
    }

    private IEnumerable<MapObject> SelectedObjects() {
        if (Objects == null || CheckboxValues == null)
            yield break;

        for (int i = 0; i < Objects.Data.Count; i++) {
            if (!CheckboxValues[i])
                continue;

            yield return Objects.Data[i];
        }
    }
}