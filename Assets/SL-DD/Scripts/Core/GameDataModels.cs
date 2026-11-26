using System;
using System.Collections.Generic;

#region Server Communication Models

[Serializable]
public class InitData
{
    public string id = "initData";
    public ServerGameData gameData;
    public ServerFeatures features;
    public ServerUIData uiData;
    public ServerPlayer player;
    public JackpotData jackpotData;
}

[Serializable]
public class JackpotData
{
    public JackpotValues values;
}

[Serializable]
public class JackpotValues
{
    public string miniJackpot;
    public string minorJackpot;
    public string majorJackpot;
    public string grandJackpot;
}

[Serializable]
public class JackpotSyncData
{
    public string gameId;
    public JackpotValues values;
}

[Serializable]
public class ServerGameData
{
    public List<List<int>> lines;
    public List<double> bets;
    public int totalLines;
}

[Serializable]
public class ServerFeatures
{
    public List<SlotJackpotTier> jackpotTiers;
    public List<List<int>> paylines; // The backend also sends these patterns in gameData.lines.
}

[Serializable]
public class SlotJackpotTier
{
    public string name;
    public int tier;
    public int? minScatters;
    public int baseMultiplier;
}

[Serializable]
public class ServerUIData
{
    public PaylineData paylines;
}

[Serializable]
public class PaylineData
{
    public List<ServerSymbolInfo> symbols;
}

[Serializable]
public class ServerSymbolInfo
{
    public int id;
    public string name;
    public List<double> multiplier;
    public string description;
}

[Serializable]
public class ServerPlayer
{
    public double balance;
}

[Serializable]
public class ServerSpinResponse
{
    public string id = "ResultData";
    public bool success;
    public List<List<string>> matrix; // Root level matrix sent by server
    public ServerPlayerBalance player;
    public ServerPayload payload;
}

[Serializable]
public class ServerPlayerBalance
{
    public double? balance; // Nullable because server sends null
}

[Serializable]
public class ServerPayload
{
    public double winAmount;

    public double linesWinAmount;
    public List<ServerLineWin> lineWins;
    public bool isFreeSpin;
    public ServerFreeSpinsResult freeSpins;
    public ServerJackpotBonusResult jackpotBonus;
}

[Serializable]
public class ServerLineWin
{
    public int lineIndex;
    public List<ServerLineWinPosition> positions;
    public List<int> pattern;
    public string symbolId;
    public string symbolName;
    public double payout;
    public int matchCount;
}

[Serializable]
public class ServerLineWinPosition
{
    public List<int> position; // [row, column]
}

[Serializable]
public class ServerFreeSpinsResult
{
    public bool triggered;
    public int spinsAdded;
    public double scatterWin;
    public int remaining;
    public double totalWon;
}

[Serializable]
public class ServerJackpotBonusResult
{
    public bool triggered;
    public int scatterCount;
    public int tier;
    public string tierName;
    public double multiplier;
    public double tierPayout;
    public double totalWon;
}

[Serializable]
public class SpinRequest
{
    public string type = "SPIN";
    public SpinPayload payload;
}

[Serializable]
public class SpinPayload
{
    public int betIndex;
    public bool isFreeSpin;
}

[Serializable]
public class JackpotOpenRequest
{
    public string type = "JACKPOT_OPEN";
    public JackpotOpenPayload payload = new JackpotOpenPayload();
}

[Serializable]
public class JackpotOpenPayload
{
  public string tier;
}

#endregion

#region Game Configuration (Client Side Converted)

[Serializable]
public class GameConfig
{
    public int reelCount = 5;
    public int rowCount = 4;
    public int symbolCount = 11;
    public int paylineCount;
    public List<List<int>> paylines;
    public List<double> availableBets;
    public List<SymbolInfo> symbols;

    internal const int DDLastReelFillerSymbolId = 8;
    internal readonly List<int> reelFillerSymbolIds = new List<int>();
    public List<SlotJackpotTier> jackpotTiers;

    public int wildSymbolId;

    // Double Diamond Bonus Diamonds are tagged "bonus" in game:init.
    public int bonusSymbolId = -1;

    public double totalBetMultiplier; // Converts the selected per-line bet to Total Pay.

    internal static int PickReelFillerSymbolId(GameConfig config)
    {
        var ids = config?.reelFillerSymbolIds;
        return ids != null && ids.Count > 0 ? ids[UnityEngine.Random.Range(0, ids.Count)] : 0;
    }
}

[Serializable]
public class SymbolInfo
{
    public int id;
    public string name;
    public List<double> multipliers;
    public bool isWild;
}

#endregion

#region Player & Game State (Client Side)

[Serializable]
public class PlayerData
{
    public double balance;
    public int currentBetIndex;
}

[Serializable]
public class SpinResult
{
    // Shared spin result and free-spin progress.
    public List<List<int>> resultMatrix; // One column per reel.
    public double winAmount;
    public List<WinLine> winLines;
    public PlayerData playerData;
    public FreeSpinData freeSpinData;
    public int serverSpinsRemaining;
    public double serverTotalRoundWin;
    public bool isRoundOver;

    public bool isFreeSpin; // True for a spin played during the free-spin round.
    public double linesWinAmount;
    public JackpotBonusData jackpotBonusData;
}

[Serializable]
public class WinLine
{
    public int lineId;
    public int symbolId;
    public List<int> positions;  // Flat list: [row * 5 + col]
    public double winAmount;
}

[Serializable]
public class FreeSpinData
{
    public bool isTriggered;
    public int spinsAwarded;
    public int remainingSpins;
    public double scatterWin; // Double Diamond trigger payout.
}

// Double Diamond Jackpot Inferno award for the current spin.
[Serializable]
public class JackpotBonusData
{
    public bool isTriggered;
    public int scatterCount;
    public int tier;
    public string tierName;
    public double multiplier;
    public double tierPayout;
    public double totalWon;
}

#endregion

#region Platform Communication

[Serializable]
public class AuthData
{
    public string token;
    public string socketURL;
    public string nameSpace;
}

#endregion

#region Enums

public enum GameState
{
    Initializing,
    Idle,
    Spinning,
    Stopping,
    ShowingWin,
    FreeSpinMode,
    SpinResultError
}

public enum SpinSpeed
{
    Normal,
    Turbo,
    QuickSpin
}

#endregion

#region Helper Classes for Conversion

/// <summary>
/// Converts Double Diamond init and spin responses to client models.
/// </summary>
public static class InitDataConverter
{
    internal static GameConfig ConvertToGameConfig(InitData serverData)
    {
        var config = new GameConfig
        {
            symbolCount = serverData.uiData.paylines.symbols.Count,
            paylineCount = serverData.gameData.totalLines,
            paylines = serverData.gameData.lines,
            availableBets = serverData.gameData.bets,
            totalBetMultiplier = serverData.gameData.totalLines,
            symbols = new List<SymbolInfo>(),
            jackpotTiers = serverData.features?.jackpotTiers
        };

        foreach (var serverSymbol in serverData.uiData.paylines.symbols)
        {
            var symbolInfo = new SymbolInfo
            {
                id = serverSymbol.id,
                name = serverSymbol.name,
                multipliers = new List<double>(),
                isWild = serverSymbol.name.ToLower().Contains("wild")
            };

            if (serverSymbol.multiplier != null)
                symbolInfo.multipliers.AddRange(serverSymbol.multiplier);

            config.symbols.Add(symbolInfo);

            // DD startup and reel scrolling share the regular symbol IDs from init.
            if (symbolInfo.id >= 0 && symbolInfo.id <= GameConfig.DDLastReelFillerSymbolId)
                config.reelFillerSymbolIds.Add(symbolInfo.id);

            if (symbolInfo.isWild)
            {
                config.wildSymbolId = symbolInfo.id;
            }
            if (string.Equals(serverSymbol.description, "bonus", StringComparison.OrdinalIgnoreCase))
            {
                config.bonusSymbolId = symbolInfo.id;
            }

        }

        return config;
    }

    internal static PlayerData ConvertToPlayerData(ServerPlayer serverPlayer, int defaultBetIndex = 0)
    {
        return new PlayerData
        {
            balance = serverPlayer.balance,
            currentBetIndex = defaultBetIndex
        };
    }

    /// <summary>
    /// Converts the spin result. The socket caller's bet is kept for signature compatibility.
    /// </summary>
    internal static SpinResult ConvertServerResponseToSpinResult(ServerSpinResponse serverResponse, double currentBalance, double betAmount, GameConfig gameConfig)
    {
        var payload = serverResponse.payload;
        var reelMatrix = ConvertMatrixToReels(serverResponse.matrix, gameConfig?.rowCount ?? 4);

        var result = new SpinResult
        {
            resultMatrix = reelMatrix,
            winAmount = payload.winAmount,
            linesWinAmount = payload.linesWinAmount,
            winLines = ConvertLineWins(payload.lineWins, reelMatrix),
            playerData = new PlayerData
            {
                // currentBalance already includes any stake deduction.
                balance = serverResponse.player?.balance ?? (currentBalance + payload.winAmount),
                currentBetIndex = 0
            },
            isFreeSpin = payload.isFreeSpin
        };

        // The jackpot award belongs to this spin. The server reports the free-spin total separately.
        if (payload.jackpotBonus != null)
        {
            var jackpot = payload.jackpotBonus;
            result.jackpotBonusData = new JackpotBonusData
            {
                isTriggered = jackpot.triggered,
                scatterCount = jackpot.scatterCount,
                tier = jackpot.tier,
                tierName = jackpot.tierName,
                multiplier = jackpot.multiplier,
                tierPayout = jackpot.tierPayout,
                totalWon = jackpot.totalWon
            };
        }

        if (payload.freeSpins != null)
        {
            var freeSpins = payload.freeSpins;
            result.serverSpinsRemaining = freeSpins.remaining;
            result.serverTotalRoundWin = freeSpins.totalWon;
            result.isRoundOver = result.isFreeSpin && freeSpins.remaining == 0;

            if (freeSpins.triggered)
            {
                result.freeSpinData = new FreeSpinData
                {
                    isTriggered = true,
                    spinsAwarded = freeSpins.spinsAdded,
                    remainingSpins = freeSpins.remaining,
                    scatterWin = freeSpins.scatterWin
                };
            }
        }

        return result;
    }

    private static List<List<int>> ConvertMatrixToReels(List<List<string>> serverMatrix, int rowCount)
    {
        if (serverMatrix == null || serverMatrix.Count == 0)
        {
            UnityEngine.Debug.LogError("Invalid server matrix: no rows were returned.");
            return GenerateDefaultMatrix(rowCount);
        }

        // The server sends rows. SlotView stores one column per reel.
        int totalRows = serverMatrix.Count;
        int totalCols = serverMatrix[0].Count;
        var reels = new List<List<int>>(totalCols);

        for (int col = 0; col < totalCols; col++)
        {
            var column = new List<int>(totalRows);
            for (int row = 0; row < totalRows; row++)
            {
                if (col >= serverMatrix[row].Count)
                {
                    UnityEngine.Debug.LogError($"Invalid server data at row {row}, col {col}");
                    column.Add(0);
                    continue;
                }

                string symbol = serverMatrix[row][col];
                if (!int.TryParse(symbol, out int symbolId))
                {
                    UnityEngine.Debug.LogError($"Failed to parse symbol: {symbol}");
                    column.Add(0);
                    continue;
                }

                column.Add(symbolId);
            }
            reels.Add(column);
        }

        return reels;
    }

    private static List<List<int>> GenerateDefaultMatrix(int rowCount)
    {
        var reels = new List<List<int>>(5);
        for (int col = 0; col < 5; col++)
        {
            var column = new List<int>(rowCount);
            for (int row = 0; row < rowCount; row++)
                column.Add(0);
            reels.Add(column);
        }

        return reels;
    }

    // Double Diamond supplies a payline index, matched cells, and a cash payout.
    private static List<WinLine> ConvertLineWins(List<ServerLineWin> serverLineWins, List<List<int>> reelMatrix)
    {
        var winLines = new List<WinLine>(serverLineWins?.Count ?? 0);
        if (serverLineWins == null)
            return winLines;

        foreach (var line in serverLineWins)
        {
            if (line?.positions == null || !int.TryParse(line.symbolId, out int symbolId))
            {
                continue;
            }

            var positions = new List<int>(line.positions.Count);
            foreach (var match in line.positions)
            {
                var coordinates = match?.position;
                if (coordinates == null || coordinates.Count != 2)
                {
                    continue;
                }

                int row = coordinates[0];
                int col = coordinates[1];
                if (col < 0 || col >= reelMatrix.Count || row < 0 || row >= reelMatrix[col].Count)
                {
                    continue;
                }

                // SlotView uses flat cell indexes with five reels per row.
                positions.Add(row * 5 + col);
            }

            if (positions.Count > 0)
            {
                winLines.Add(new WinLine
                {
                    lineId = line.lineIndex,
                    symbolId = symbolId,
                    positions = positions,
                    winAmount = line.payout
                });
            }
        }

        return winLines;
    }
}

#endregion
