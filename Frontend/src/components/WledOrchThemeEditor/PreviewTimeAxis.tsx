import { Box, Text } from "@chakra-ui/react";
import { useEffect, useRef, useState } from "react";
import {
  getTimeAxisTicks,
  PREVIEW_AXIS_VERTICAL_PADDING_PX,
} from "./previewTimeAxisTicks";

interface Props {
  previewType: unknown;
}

/**
 * Axis tick labels for the previews time axis. The time axis is the y axis of the preview image
 * (downward space is used for time resolution), so this renders as a vertical label strip left of
 * the image. The strip stretches with the image; ticks are spread with identical vertical spacing
 * over the measured strip height (with a small padding so the first/last labels stay inside).
 */
const PreviewTimeAxis = ({ previewType }: Props) => {
  const gutterRef = useRef<HTMLDivElement | null>(null);
  const [heightPx, setHeightPx] = useState(0);

  useEffect(() => {
    const gutter = gutterRef.current;
    if (!gutter) return;

    const updateHeight = () => setHeightPx(gutter.clientHeight);
    updateHeight();
    const observer = new ResizeObserver(updateHeight);
    observer.observe(gutter);
    return () => observer.disconnect();
  }, []);

  // The tick step is chosen from the measured height so labels never overlap, even on short previews.
  const ticks = getTimeAxisTicks(previewType, heightPx);
  const usableHeight = Math.max(
    0,
    heightPx - 2 * PREVIEW_AXIS_VERTICAL_PADDING_PX
  );

  return (
    <Box
      ref={gutterRef}
      position="relative"
      width="46px"
      flexShrink={0}
      aria-hidden={ticks.length === 0 || undefined}
      visibility={ticks.length === 0 ? "hidden" : "visible"}
    >
      {ticks.map((tick) => {
        const centerY =
          PREVIEW_AXIS_VERTICAL_PADDING_PX + tick.topFraction * usableHeight;
        return (
          <Text
            key={tick.value}
            position="absolute"
            right={2}
            // Pass an explicit px string: Chakra maps *numeric* top/left/etc. through its space
            // scale (top={8} becomes var(--chakra-space-8) = 32px), which would throw the 0-tick
            // label far off its intended position.
            top={`${Math.round(centerY)}px`}
            transform="translateY(-50%)"
            fontSize="10px"
            lineHeight={1}
            color="gray.500"
            whiteSpace="nowrap"
          >
            {tick.label}
          </Text>
        );
      })}
    </Box>
  );
};

export default PreviewTimeAxis;
