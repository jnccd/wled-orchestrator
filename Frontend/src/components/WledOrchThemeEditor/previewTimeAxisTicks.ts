export interface TimeAxisTick {
  value: number;
  label: string;
  /** 0 = top of the image (start of the preview window), 1 = bottom (end of the window). */
  topFraction: number;
}

/**
 * The preview image renders one row per time step: the top row is the start of the preview window
 * and the bottom row is its end (the server maps LedThemePreviewType to that window, see
 * LedTheme.GetPreviewState). Which window applies depends on the theme's LedThemePreviewType:
 *   Day: 24 hours
 *   Min: 60 seconds
 *
 * Ticks are placed with uniform vertical spacing across the axis. The tick step thins out when the
 * strip is too short to show the preferred step without the labels overlapping.
 */
export const PREVIEW_AXIS_VERTICAL_PADDING_PX = 8;
const MIN_TICK_SPACING_PX = 16;

interface TickSpec {
  total: number;
  preferredStep: number;
  /** Possible tick steps, ascending; the first one that still fits is used. */
  allowedSteps: number[];
  label: (value: number) => string;
}

const previewTypeTickSpecs: Record<string, TickSpec> = {
  Day: {
    total: 24,
    preferredStep: 3,
    allowedSteps: [1, 2, 3, 4, 6, 8, 12, 24],
    label: (value) => `${String(value).padStart(2, "0")}:00`,
  },
  Min: {
    total: 60,
    preferredStep: 10,
    allowedSteps: [1, 2, 3, 4, 5, 6, 10, 12, 15, 20, 30, 60],
    label: (value) => `${value}s`,
  },
};

const toPreviewTypeKey = (previewType: unknown): string => {
  // LedThemePreviewType is a .NET enum; /state currently serializes it numerically (0/1) but may
  // later use strings, so accept both.
  if (previewType === "Day" || previewType === 0 || previewType === "0") return "Day";
  if (previewType === "Min" || previewType === 1 || previewType === "1") return "Min";
  if (typeof previewType === "string") return previewType; // future types serialized as strings
  return "";
};

export const getTimeAxisTicks = (
  previewType: unknown,
  heightPx: number
): TimeAxisTick[] => {
  if (!(heightPx > 0)) return [];

  const spec = previewTypeTickSpecs[toPreviewTypeKey(previewType)];
  if (!spec) return [];

  const usableHeight = Math.max(
    0,
    heightPx - 2 * PREVIEW_AXIS_VERTICAL_PADDING_PX
  );
  // Step that guarantees at least MIN_TICK_SPACING_PX between adjacent tick centers.
  const minimumFittingStep = (MIN_TICK_SPACING_PX * spec.total) / usableHeight;
  const step =
    spec.allowedSteps.find(
      (candidate) => candidate >= Math.max(spec.preferredStep, minimumFittingStep)
    ) ?? spec.allowedSteps[spec.allowedSteps.length - 1];

  const ticks: TimeAxisTick[] = [];
  for (let value = 0; value <= spec.total; value += step)
    ticks.push({ value, label: spec.label(value), topFraction: value / spec.total });
  return ticks;
};
