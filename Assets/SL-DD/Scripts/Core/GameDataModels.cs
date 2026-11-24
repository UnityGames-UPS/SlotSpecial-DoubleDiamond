using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json; // Legacy ExtraSpinsData uses JsonProperty.

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
    public JackpotValues values;//jackpotfeature

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
    // Legacy init field. Double Diamond uses totalLines for the total bet multiplier.
    // public double creditDivisor = 25;
    public int totalLines;
}

[Serializable]
public class ServerFeatures
{
    public List<SlotJackpotTier> jackpotTiers;
    public List<List<int>> paylines; // The backend also sends these patterns in gameData.lines.

    // Legacy template init fields.
    /*
    public USpinFeature uSpin;
    public MoneyBagFeature moneyBag;
    public FreeGamesFeature freeGames;
    public int betMultiplier;
    public int maxWinMultiplier;
    public int minWinMultiplier;
    */
}

[Serializable]
public class SlotJackpotTier
{
    public string name;
    public int tier;
    public int? minScatters;
    // Old DD init field. All current tiers use minScatters.
    // public bool? jumpOnly;
    public int baseMultiplier;
}

// Legacy template U-Spin init DTO.
/*
[Serializable]
public class USpinFeature
{
    public bool enabled;
    public int minTrigger;
    public int symbolId;
    public List<USpinSegment> segments;
}
*/

// GameConfig and WheelSpinController still use this type.
[Serializable]
public class USpinSegment
{
    public int sliceIndex;
    public string type;
    public double multiplier;
    public int freeGames;
}

// Legacy template bonus settings and extra-spin lookup.
/*
[Serializable]
public class MoneyBagFeature
{
    public bool enabled;
    public int minTrigger;
    public int symbolId;
    public int bagCount;
}

[Serializable]
public class FreeGamesFeature
{
    public bool enabled;
    public double payMultiplier;
    public int maxTotalFreeGames;
}

[Serializable]
public class ExtraSpinsData
{
    [JsonProperty("2")] public int _2; // Keep for safety/compatibility with UI
    [JsonProperty("3")] public int _3;
    [JsonProperty("4")] public int _4;
    [JsonProperty("5")] public int _5;
}
*/

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

    // Legacy template paytable fields.
    // public List<double> payout;
    // public int minMatch;
}


[Serializable]
public class ServerPlayer
{
    public double balance;
}

// ============================================================================
// FIXED: Server Response Models - Must match actual server JSON structure
// ============================================================================

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

    // Double Diamond result fields.
    public double linesWinAmount;
    public List<ServerLineWin> lineWins;
    public bool isFreeSpin;
    public ServerFreeSpinsResult freeSpins;

    // Legacy CNY result fields. The current converter still reads these.
    public List<List<string>> reels;
    public double totalWin;
    public int scatterCount;
    public bool scatterTriggered;
    public bool isRoundOver;
    public double totalRoundWin;
    public double grandTotalWin;
    public double netReturnRatio;
    public List<ServerWaysWin> waysWins;
    public ServerUSpinResult uSpin;
    public ServerMoneyBagResult moneyBag;
    public ServerFreeGamesResult freeGames;
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
public class ServerWaysWin
{
    public int symbolId;
    public int matchCount;
    public int waysCount;
    public List<ServerPosition> matchedPositions;
    public double basePayout;
    public double appliedMultiplier;
    public double winInCredits;
    public double winInCash;
    public string winType;
}

[Serializable]
public class ServerPosition
{
    public int row;
    public int col;
}

[Serializable]
public class ServerUSpinResult
{
    public bool triggered;
    public ServerUSpinResultDetail result;
}

[Serializable]
public class ServerUSpinResultDetail
{
    public int sliceIndex;
    public string type;
    public double multiplierAwarded;
    public int freeGamesAwarded;
    public double winInCash;
}

[Serializable]
public class ServerMoneyBagResult
{
    public bool triggered;
    public ServerMoneyBagResultDetail result;
}

[Serializable]
public class ServerMoneyBagResultDetail
{
    public int pickedIndex;
    public List<int> revealed;
    public int creditsAwarded;
    public double winInCash;
}

[Serializable]
public class ServerFreeGamesResult
{
    public bool triggered;
    public int totalAwarded;
    public int played;
    public double totalFreeGamesWin;
}

// ============================================================================
// Client-Side Spin Request
// ============================================================================

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
    public int rowCount = 3;
    public int symbolCount = 11;
    public int paylineCount = 243;
    public List<List<int>> paylines;
    public List<double> availableBets;
    public List<SymbolInfo> symbols;

    internal const int DDLastReelFillerSymbolId = 8;
    internal readonly List<int> reelFillerSymbolIds = new List<int>();
    public List<SlotJackpotTier> jackpotTiers;

    // Wild configuration
    public int wildSymbolId = 10;      // Base wild (10)

    // Double Diamond Bonus Diamonds are tagged "bonus" in game:init.
    public int bonusSymbolId = -1;

    // Legacy CNY scatter configuration.
    public int scatterSymbolId = 11; // U-Spin.

    public double totalBetMultiplier = 25; // Converts the selected bet to Total Pay.

    // Legacy runtime fields with no gameplay readers.
    /*
    public int betMultiplier = 1;
    public int maxWinMultiplier = 10000;
    public int minWinMultiplier = 10;
    public int initialFreeSpins = 12;
    public ExtraSpinsData extraSpinsData;
    */

    // uSpin
    public List<USpinSegment> uSpinSegments;

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
    public bool isScatter;

    // Legacy symbol fields with no gameplay readers.
    // public int wildMultiplier = 1;
    // public int minMatch;
}

#endregion

#region Player & Game State (Client Side)

// Shared runtime models.
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
    public List<List<int>> resultMatrix;  // Client uses int matrix
    public double winAmount;
    public double grandTotalWin;
    public List<WinLine> winLines;
    public PlayerData playerData;
    public FreeSpinData freeSpinData;
    public int serverSpinsRemaining;
    public int serverSpinsUsed;
    public int serverTotalSpins;
    public double serverTotalRoundWin;
    public bool isRoundOver;

    // Double Diamond result fields.
    public bool isDoubleDiamondResult;
    public bool isFreeSpin; // True for a spin played during the free-spin round.

    // Legacy CNY feature fields and payout helpers.
    public ScatterData scatterData;
    public OverlayScatterData overlayScatterData;
    public Dictionary<string, int> stickyWilds;
    public USpinResultData uSpinData;
    public MoneyBagResultData moneyBagData;

    public double GetMoneyBagWin()
    {
        return (moneyBagData != null && moneyBagData.triggered) ? moneyBagData.winInCash : 0;
    }

    public double GetUSpinCashWin()
    {
        return (uSpinData != null && uSpinData.triggered && uSpinData.type == "MULTIPLIER") ? uSpinData.winInCash : 0;
    }

    public double GetTotalFeatureDeferredWins()
    {
        return GetMoneyBagWin() + GetUSpinCashWin();
    }
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

    // Legacy CNY purchase flag.
    public bool isBought;
}

// Legacy CNY feature models.
[Serializable]
public class ScatterData
{
    public bool isTriggered;
    public int scatterCount;
    public double winAmount;
}

[Serializable]
public class OverlayScatterData
{
    public bool isTriggered;
    public int count;
    public int extraSpins;
    public List<List<int>> positions;
}

[Serializable]
public class USpinResultData
{
    public bool triggered;
    public int sliceIndex;
    public string type;
    public double multiplierAwarded;
    public int freeGamesAwarded;
    public double winInCash;
}

[Serializable]
public class MoneyBagResultData
{
    public bool triggered;
    public int pickedIndex;
    public List<int> revealed;
    public int creditsAwarded;
    public double winInCash;
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

public enum WinPopupType
{
    RegularWin,         // Normal credit win (multiplier < 500x)
    BigWin,             // Big win (multiplier >= 500x)
    FreeSpinTrigger,    // Free spins awarded from wheel
    MoneyBagCollect,    // Money bag feature collect
    FreeSpinComplete    // All free spins completed
}

#endregion

#region Helper Classes for Conversion

/// <summary>
/// Converts server data to client GameConfig
/// </summary>
public static class InitDataConverter
{
    internal static GameConfig ConvertToGameConfig(InitData serverData)
    {
        var config = new GameConfig
        {
            reelCount = 5,
            rowCount = 4,
            symbolCount = serverData.uiData.paylines.symbols.Count,
            paylineCount = serverData.gameData.totalLines,
            paylines = serverData.gameData.lines,
            availableBets = serverData.gameData.bets,
            totalBetMultiplier = serverData.gameData.totalLines,
            symbols = new List<SymbolInfo>()
        }; 

        foreach (var serverSymbol in serverData.uiData.paylines.symbols)
        {
            var symbolInfo = new SymbolInfo
            {
                id = serverSymbol.id,
                name = serverSymbol.name,
                multipliers = new List<double>(),
                isWild = serverSymbol.name.ToLower().Contains("wild"),
                isScatter = serverSymbol.name.ToLower().Contains("scatter") || 
                            serverSymbol.name.ToLower().Contains("uspin") || 
                            serverSymbol.name.ToLower().Contains("moneybag"),
                // Legacy paytable mapping:
                // minMatch = serverSymbol.minMatch
            };

            if (serverSymbol.multiplier != null)
            {
                symbolInfo.multipliers.AddRange(serverSymbol.multiplier);
            }
            // Legacy payout arrays required reversing to put the highest match count first.
            /*
            else if (serverSymbol.payout != null)
            {
                for (int i = serverSymbol.payout.Count - 1; i >= 0; i--)
                {
                    symbolInfo.multipliers.Add(serverSymbol.payout[i]);
                }
            }
            */

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

            // Legacy CNY identifies its scatter by the U-Spin name.
            if (symbolInfo.isScatter && symbolInfo.name.ToLower().Contains("uspin"))
            {
                config.scatterSymbolId = symbolInfo.id;
            }
        }

        if (serverData.features != null)
        {
            config.jackpotTiers = serverData.features.jackpotTiers;

            // Legacy template init mapping. Spin results have separate DTOs and conversion.
            /*
            config.betMultiplier = serverData.features.betMultiplier > 0 ? serverData.features.betMultiplier : 1;
            config.maxWinMultiplier = serverData.features.maxWinMultiplier;
            config.minWinMultiplier = serverData.features.minWinMultiplier;

            if (serverData.features.freeGames != null)
            {
                config.initialFreeSpins = serverData.features.freeGames.maxTotalFreeGames;
            }

            if (serverData.features.uSpin != null && serverData.features.uSpin.segments != null)
            {
                config.uSpinSegments = serverData.features.uSpin.segments;
            }
            */
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
    /// Converts server response to client SpinResult
    /// </summary>
    internal static SpinResult ConvertServerResponseToSpinResult(ServerSpinResponse serverResponse, double currentBalance, double betAmount, GameConfig gameConfig)
    {
        var payload = serverResponse.payload;
        bool isDoubleDiamondResult = payload.lineWins != null || payload.freeSpins != null;
        double winAmount = isDoubleDiamondResult || payload.winAmount > 0 ? payload.winAmount : payload.totalWin;
        double totalPay = (gameConfig != null && gameConfig.totalBetMultiplier > 0) ? betAmount * gameConfig.totalBetMultiplier : betAmount * 25;
        double newBalance = serverResponse.player?.balance ?? CalculateNewBalance(currentBalance, totalPay, winAmount);
        var reelMatrix = ConvertReelsToMatrix(payload.reels, serverResponse.matrix, payload.waysWins, gameConfig);

        var result = new SpinResult
        {
            resultMatrix = reelMatrix,
            winAmount = winAmount,
            grandTotalWin = winAmount,
            // A present Double Diamond list is authoritative, even when empty.
            winLines = payload.lineWins != null
                ? ConvertLineWins(payload.lineWins, reelMatrix)
                : ConvertWinningLines(payload.waysWins, gameConfig),
            playerData = new PlayerData
            {
                balance = newBalance,
                currentBetIndex = 0
            },
            isDoubleDiamondResult = isDoubleDiamondResult,
            isFreeSpin = payload.isFreeSpin
        };

        // Double Diamond sends remaining spins and the running round total directly.
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

                // On the paid trigger spin, the award is the initial round size.
                // Later played and awarded counts require round history.
                if (!result.isFreeSpin)
                {
                    result.serverTotalSpins = freeSpins.spinsAdded;
                }
            }
        }
        // Legacy CNY sends played and awarded counts instead of remaining spins.
        else if (payload.freeGames != null)
        {
            var freeGames = payload.freeGames;
            result.serverSpinsRemaining = freeGames.totalAwarded - freeGames.played;
            result.serverSpinsUsed = freeGames.played;
            result.serverTotalSpins = freeGames.totalAwarded;
            result.serverTotalRoundWin = freeGames.totalFreeGamesWin;
            result.isRoundOver = freeGames.played >= freeGames.totalAwarded && freeGames.totalAwarded > 0;

            if (freeGames.triggered)
            {
                result.freeSpinData = new FreeSpinData
                {
                    isTriggered = true,
                    spinsAwarded = freeGames.totalAwarded,
                    remainingSpins = result.serverSpinsRemaining,
                    isBought = false
                };
            }
        }
        else
        {
            // Legacy CNY responses can report round completion at the payload level.
            result.isRoundOver = payload.isRoundOver;
            result.serverTotalRoundWin = payload.totalRoundWin;
        }

        // Legacy CNY scatter, wheel, and Money Bag results.
        if (!isDoubleDiamondResult)
        {
            result.grandTotalWin = payload.grandTotalWin > 0
                ? payload.grandTotalWin
                : winAmount + (payload.moneyBag?.result?.winInCash ?? 0) + (payload.uSpin?.result?.winInCash ?? 0);

            result.scatterData = payload.scatterTriggered
                ? new ScatterData
                {
                    isTriggered = true,
                    scatterCount = payload.scatterCount,
                    winAmount = 0
                }
                : null;

            result.overlayScatterData = null;
            result.stickyWilds = null;

            result.uSpinData = (payload.uSpin != null && payload.uSpin.triggered && payload.uSpin.result != null)
                ? new USpinResultData
                {
                    triggered = true,
                    sliceIndex = payload.uSpin.result.sliceIndex,
                    type = payload.uSpin.result.type,
                    multiplierAwarded = payload.uSpin.result.multiplierAwarded,
                    freeGamesAwarded = payload.uSpin.result.freeGamesAwarded,
                    winInCash = payload.uSpin.result.winInCash
                }
                : null;

            result.moneyBagData = (payload.moneyBag != null && payload.moneyBag.triggered && payload.moneyBag.result != null)
                ? new MoneyBagResultData
                {
                    triggered = true,
                    pickedIndex = payload.moneyBag.result.pickedIndex,
                    revealed = payload.moneyBag.result.revealed,
                    creditsAwarded = payload.moneyBag.result.creditsAwarded,
                    winInCash = payload.moneyBag.result.winInCash
                }
                : null;
        }

        return result;
    }

    private static List<List<int>> ConvertReelsToMatrix(List<List<string>> serverReels, List<List<string>> serverMatrix, List<ServerWaysWin> waysWins, GameConfig gameConfig)
    {
        var sourceReels = serverMatrix ?? serverReels;
        int rowCount = gameConfig != null ? gameConfig.rowCount : 3;

        if (sourceReels == null || sourceReels.Count == 0)
        {
            UnityEngine.Debug.LogError("Invalid server reels/matrix: sourceReels is null or empty");
            return GenerateDefaultMatrix(rowCount);
        }

        int totalRows = sourceReels.Count;
        int totalCols = sourceReels[0].Count;

        var matrix = new List<List<int>>();

        for (int col = 0; col < totalCols; col++)
        {
            var column = new List<int>();
            for (int row = 0; row < totalRows; row++)
            {
                if (col >= sourceReels[row].Count)
                {
                    UnityEngine.Debug.LogError($"Invalid server data at row {row}, col {col}");
                    column.Add(0);
                    continue;
                }

                string symbolStr = sourceReels[row][col];
                if (!int.TryParse(symbolStr, out int symbolId))
                {
                    UnityEngine.Debug.LogError($"Failed to parse symbol: {symbolStr}");
                    column.Add(0);
                    continue;
                }

                column.Add(symbolId);
            }
            matrix.Add(column);
        }

        return matrix;
    }

    private static List<List<int>> GenerateDefaultMatrix(int rowCount)
    {
        var matrix = new List<List<int>>();
        for (int col = 0; col < 5; col++)
        {
            var column = new List<int>();
            for (int row = 0; row < rowCount; row++)
            {
                column.Add(0);
            }
            matrix.Add(column);
        }
        return matrix;
    }

    // Double Diamond supplies a payline index, matched cells, and a cash payout.
    private static List<WinLine> ConvertLineWins(List<ServerLineWin> serverLineWins, List<List<int>> reelMatrix)
    {
        var winLines = new List<WinLine>(serverLineWins.Count);

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

    // Legacy CNY ways wins use their returned order as the line index.
    private static List<WinLine> ConvertWinningLines(List<ServerWaysWin> serverWaysWins, GameConfig gameConfig)
    {
        var winLines = new List<WinLine>();
        if (serverWaysWins == null) return winLines;

        int index = 0;
        foreach (var waysWin in serverWaysWins)
        {
            var flatPositions = new List<int>();
            if (waysWin.matchedPositions != null)
            {
                foreach (var pos in waysWin.matchedPositions)
                {
                    int flatIndex = pos.row * 5 + pos.col;
                    flatPositions.Add(flatIndex);
                }
            }

            winLines.Add(new WinLine
            {
                lineId = index++,
                symbolId = waysWin.symbolId,
                positions = flatPositions,
                winAmount = waysWin.winInCash
            });
        }

        return winLines;
    }

    private static double CalculateNewBalance(double currentBalance, double totalPay, double winAmount)
    {
        return currentBalance + winAmount;
    }
}

#endregion
