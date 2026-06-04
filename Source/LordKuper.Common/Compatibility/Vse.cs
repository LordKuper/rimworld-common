using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace LordKuper.Common.Compatibility;

/// <summary>
///     Provides compatibility integration with the Vanilla Skills Expanded (VSE) mod.
/// </summary>
public static class Vse
{
    /// <summary>
    ///     Delegate type for retrieving all <see cref="Passion" /> values from VSE.
    /// </summary>
    public delegate IEnumerable<Passion> GetPassionsDelegate();

    /// <summary>
    ///     Delegate for retrieving all available passions from VSE.
    /// </summary>
    public static GetPassionsDelegate? GetPassions;

    /// <summary>
    ///     Cache for mapping <see cref="Passion" /> to their corresponding VSE passion definitions.
    /// </summary>
    private static readonly Dictionary<Passion, object> PassionDefs = new();

    /// <summary>
    ///     Indicates whether the Vanilla Skills Expanded mod is active.
    /// </summary>
    public static bool VanillaSkillsExpandedActive;

    /// <summary>
    ///     FieldInfo for the 'forgetRateFactor' field in VSE's PassionDef.
    /// </summary>
    private static FieldInfo? _forgetRateFactorField;

    /// <summary>
    ///     PropertyInfo for the 'Icon' property in VSE's PassionDef.
    /// </summary>
    private static PropertyInfo? _iconProperty;

    /// <summary>
    ///     Indicates whether the compatibility has been initialized.
    /// </summary>
    private static bool _isInitialized;

    /// <summary>
    ///     FieldInfo for the 'learnRateFactor' field in VSE's PassionDef.
    /// </summary>
    private static FieldInfo? _learnRateFactorField;

    /// <summary>
    ///     Delegate for converting a <see cref="Passion" /> to its corresponding <see cref="Def" /> in VSE.
    /// </summary>
    private static PassionToDefDelegate? _passionToDef;

    /// <summary>
    ///     Gets the VSE <see cref="Def" /> for the specified <see cref="Passion" />.
    /// </summary>
    /// <param name="passion">The passion to convert.</param>
    /// <returns>The corresponding <see cref="Def" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the conversion fails or the result is not a Def.</exception>
    private static Def GetDef(Passion passion)
    {
        var passionDef = GetPassionDef(passion);
        if (passionDef is Def def) return def;
        throw new InvalidOperationException(
            $"Passion '{passion}' converted to non-Def type: {passionDef.GetType().Name}");
    }

    /// <summary>
    ///     Gets the defName of the VSE <see cref="Def" /> for the specified <see cref="Passion" />.
    /// </summary>
    /// <param name="passion">The passion to query.</param>
    /// <returns>The defName string.</returns>
    public static string GetDefName(Passion passion)
    {
        return GetDef(passion).defName;
    }

    /// <summary>
    ///     Gets the forget rate factor for the specified <see cref="Passion" /> from VSE.
    /// </summary>
    /// <param name="passion">The passion to query.</param>
    /// <returns>The forget rate factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the field is not initialized or not a float.</exception>
    public static float GetForgetRateFactor(Passion passion)
    {
        var def = GetDef(passion);
        if (_forgetRateFactorField == null)
            throw new InvalidOperationException("ForgetRateFactor field is not initialized.");
        if (_forgetRateFactorField.GetValue(def) is float forgetRateFactor) return forgetRateFactor;
        throw new InvalidOperationException(
            $"ForgetRateFactor field is not of type float in passion definition: {def.defName}");
    }

    /// <summary>
    ///     Gets the icon for the specified <see cref="Passion" /> from VSE.
    /// </summary>
    /// <param name="passion">The passion to query.</param>
    /// <returns>The icon as a <see cref="Texture2D" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the property is not initialized or not a Texture2D.</exception>
    public static Texture2D GetIcon(Passion passion)
    {
        var passionDef = GetPassionDef(passion);
        if (_iconProperty == null)
            throw new InvalidOperationException("Icon property is not initialized.");
        if (_iconProperty.GetValue(passionDef) is Texture2D icon) return icon;
        throw new InvalidOperationException(
            $"Icon property is not of type Texture2D in passion definition: {GetDefName(passion)}");
    }

    /// <summary>
    ///     Gets the label for the specified <see cref="Passion" /> from VSE.
    /// </summary>
    /// <param name="passion">The passion to query.</param>
    /// <returns>The label string.</returns>
    public static string GetLabel(Passion passion)
    {
        var def = GetDef(passion);
        return string.IsNullOrEmpty(def.LabelCap) ? def.defName : def.LabelCap;
    }

    /// <summary>
    ///     Gets the learn rate factor for the specified <see cref="Passion" /> from VSE.
    /// </summary>
    /// <param name="passion">The passion to query.</param>
    /// <returns>The learn rate factor.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the field is not initialized or not a float.</exception>
    public static float GetLearnRateFactor(Passion passion)
    {
        var def = GetDef(passion);
        if (_learnRateFactorField == null)
            throw new InvalidOperationException("LearnRateFactor field is not initialized.");
        if (_learnRateFactorField.GetValue(def) is float learnRateFactor) return learnRateFactor;
        throw new InvalidOperationException(
            $"LearnRateFactor field is not of type float in passion definition: {def.defName}");
    }

    /// <summary>
    ///     Gets the VSE passion definition object for the specified <see cref="Passion" />.
    /// </summary>
    /// <param name="passion">The passion to convert.</param>
    /// <returns>The corresponding VSE passion definition object.</returns>
    /// <exception cref="InvalidOperationException">Thrown if the delegate is not initialized or conversion fails.</exception>
    private static object GetPassionDef(Passion passion)
    {
        if (PassionDefs.TryGetValue(passion, out var passionDef)) return passionDef;
        if (_passionToDef == null)
            throw new InvalidOperationException("PassionToDef delegate is not initialized.");
        passionDef = _passionToDef(passion);
        PassionDefs[passion] = passionDef ??
                               throw new InvalidOperationException(
                                   $"Failed to convert passion '{passion}' to PassionDef.");
        return passionDef;
    }

    /// <summary>
    ///     Initializes compatibility with Vanilla Skills Expanded.
    /// </summary>
    internal static void Initialize()
    {
        if (_isInitialized) return;
        _isInitialized = true;
        VanillaSkillsExpandedActive = LoadedModManager.RunningModsListForReading.Any(m =>
            "vanillaexpanded.skills".Equals(m.PackageId, StringComparison.OrdinalIgnoreCase));
        if (!VanillaSkillsExpandedActive) return;
#if DEBUG
        Logger.LogMessage("Vanilla Skills Expanded detected.");
#endif
        try
        {
            var passionManager = AccessTools.TypeByName("VSE.Passions.PassionManager") ??
                                 throw new InvalidOperationException(
                                     "Could not find 'PassionManager' type.");
            GetPassions = AccessTools.MethodDelegate<GetPassionsDelegate>(
                AccessTools.PropertyGetter(passionManager, "AllPassions"));
            if (GetPassions == null)
                throw new InvalidOperationException(
                    "Could not create 'GetPassions' method delegate.");
            _passionToDef = AccessTools.MethodDelegate<PassionToDefDelegate>(
                AccessTools.Method(passionManager, "PassionToDef"));
            if (_passionToDef == null)
                throw new InvalidOperationException(
                    "Could not create 'PassionToDef' method delegate.");
            var passionDef = AccessTools.TypeByName("VSE.Passions.PassionDef") ??
                             throw new InvalidOperationException(
                                 "Could not find 'PassionDef' type.");
            _learnRateFactorField = AccessTools.Field(passionDef, "learnRateFactor");
            if (_learnRateFactorField == null)
                throw new InvalidOperationException(
                    "Could not find 'learnRateFactor' field in 'PassionDef'.");
            if (_learnRateFactorField.FieldType != typeof(float))
                throw new InvalidOperationException(
                    $"Expected 'learnRateFactor' field to be of type float, but found {_learnRateFactorField.FieldType.Name}.");
            _forgetRateFactorField = AccessTools.Field(passionDef, "forgetRateFactor");
            if (_forgetRateFactorField == null)
                throw new InvalidOperationException(
                    "Could not find 'forgetRateFactor' field in 'PassionDef'.");
            if (_forgetRateFactorField.FieldType != typeof(float))
                throw new InvalidOperationException(
                    $"Expected 'forgetRateFactor' field to be of type float, but found {_forgetRateFactorField.FieldType.Name}.");
            _iconProperty = AccessTools.Property(passionDef, "Icon");
            if (_iconProperty == null)
                throw new InvalidOperationException(
                    "Could not find 'Icon' property in 'PassionDef'.");
            if (_iconProperty.PropertyType != typeof(Texture2D))
                throw new InvalidOperationException(
                    $"Expected 'Icon' property to be of type Texture2D, but found {_iconProperty.PropertyType.Name}.");
        }
        catch (Exception e)
        {
            Logger.LogError("Failed to initialize VSE compatibility.", e);
        }
    }

    /// <summary>
    ///     Delegate type for converting a <see cref="Passion" /> to a <see cref="Def" /> in VSE.
    /// </summary>
    /// <param name="passion">The passion to convert.</param>
    /// <returns>The corresponding <see cref="Def" />.</returns>
    private delegate object? PassionToDefDelegate(Passion passion);
}