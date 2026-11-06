# CHANGELOG

Bu dosya [Keep a Changelog](https://keepachangelog.com/tr/1.1.0/) biçimini izler;
sürümler [Semantic Versioning](https://semver.org/lang/tr/) kurallarına göre numaralanır.

## [Unreleased]

## [1.1.0] - 2026-11-06

### Added

- order: talep özeti uç noktası (`GET /api/orders/summary`)
- ci: Jenkins pipeline, SonarQube Quality Gate, test ortamına otomatik deploy

### Changed

- common-contracts: servisler sözleşmeleri Nexus'taki NuGet paketinden alır

### Fixed

- order: miktarı 1'den küçük talep açılamaz (#7)

## [1.0.1] - 2026-11-03

### Fixed

- order: boş ekipman koduyla talep açılamaz (#2)

## [1.0.0] - 2026-11-03

### Added

- catalog: stok durumu alanı (#1)
- catalog: SRJ-04 şarj ünitesi
