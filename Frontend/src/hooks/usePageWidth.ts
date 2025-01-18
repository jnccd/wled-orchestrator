import { useEffect, useState } from "react";

export const usePageWidth = () => {
    const [pageWidth, setPageWidth] = useState(window.innerWidth); 

    useEffect(() => { 
        const handleResize = () => { 
            setPageWidth(window.innerWidth); 
        }; 
        window.addEventListener('resize', handleResize); 
        return () => 
            { 
                window.removeEventListener('resize', handleResize); 
            }; 
        }, 
    []);

    return pageWidth;
}