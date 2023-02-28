# PiratenKarte TODO:
## TODO
- Rechtesystem (Wie bei ShopDev per vererbung/Rechtegruppen)
- Gruppen (User ist teil von)
- Sichtbarkeit (Öffentlich, BV, LV, KV ...)

- Filter auf list
- MoveManyToStorage => Modal
- Http results prüfen
- Toasts für erfolgreich/fehler: https://github.com/Blazored/Toast
- GPS für map: https://www.nuget.org/packages/Blazor.Geolocation.WebAssembly

## Zukunft
- WaWi

## Issues
- Make comments use logged in user
- Move admin password to settings.json
- move login/auth logic to new service
- Check login if loaded from local storage
- Coordinates need to be clamped (or modulo'd) to world space
- Try to clamp map position to sensible world
- Add cleanup routine for invalid tokens
- Add custom input component for lat, lon (which can direct to map for selection)
- Add custom input component for select/combobox (should include auto-complete)
- Add password features to FormInput