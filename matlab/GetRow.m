function row = GetRow( m, t1, t2 )  
    range1 = m(m(:,8) < t1 & m(:,7) > 0,:);
    range2 = m(m(:,8) > t1 & m(:,8) < t2,:);
    range3 = m(m(:,8) > t2,:);
    T1 = mean(range1(:,7));
    T2 = mean(range2(:,7));
    T3 = mean(range3(:,7));
    
    E1 = mean(range1(:,3) + range1(:,4) + range1(:,6));
    E2 = mean(range2(:,3) + range2(:,4) + range2(:,6));
    E3 = mean(range3(:,3) + range3(:,4) + range3(:,6));
    
    row = [t1, t2, T1, T2, T3, E1, E2, E3];    
end

