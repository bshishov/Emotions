function [ L, M, H ] = GetPhasesFromEngine(l,m,h, X, t1, t2 )
    fps = 10;
    i1 = fps * t1 / 1000;
    i2 = fps * t2 / 1000;   
    
    c11 = int16(0.1 * (i1 - 1) + 1);
    c12 = int16(0.4 * (i1 - 1) + 1);
    
    c21 = int16(0.1 * (i2 - i1) + i1);
    c22 = int16(0.4 * (i2 - i1) + i1);
    
    c31 = int16(0.5 * (size(X,1) - i2) + i2);
    c32 = int16(0.9 * (size(X,1) - i2) + i2);
       
    r1 = X([c11:c12],:);
    r2 = X([c21:c22],:);
    r3 = X([c31:c32],:);
    
    normalState = mean(r1);
    
    for i=1:size(X,2)
        r1(:,i) = r1(:,i) - normalState(:,i);
        r2(:,i) = r2(:,i) - normalState(:,i);
        r3(:,i) = r3(:,i) - normalState(:,i);
    end
    
    %L = vertcat(l, r1);
    %M = vertcat(m, r2);
    %H = vertcat(h, r3);    
    
    L = vertcat(l, X([c11:c12],:));
    M = vertcat(m, X([c21:c22],:));
    H = vertcat(h, X([c31:c32],:));    
end

