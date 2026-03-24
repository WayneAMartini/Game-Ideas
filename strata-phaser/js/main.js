// STRATA - Main Entry Point
// ===========================
(function(S) {
    'use strict';

    var config = {
        type: Phaser.AUTO,
        width: S.WIDTH,
        height: S.HEIGHT,
        backgroundColor: '#0a0a0a',
        pixelArt: true,
        physics: {
            default: 'arcade',
            arcade: {
                gravity: { y: S.PLAYER.gravity },
                debug: false
            }
        },
        scale: {
            mode: Phaser.Scale.FIT,
            autoCenter: Phaser.Scale.CENTER_BOTH
        },
        scene: [
            S.BootScene,
            S.MenuScene,
            S.GameScene,
            S.PrepScene,
            S.SummaryScene,
            S.MarketScene,
            S.UpgradeScene,
            S.ContractScene,
            S.InventoryScene
        ]
    };

    S.game = new Phaser.Game(config);

})(window.STRATA);
